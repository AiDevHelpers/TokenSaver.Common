using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace AiDevTools.TokenSaver.Common.Models;

/// <summary>Why an edit of a client config did not happen. Stable codes, recorded in docs/Host-MCP/provider-config-reliability.md.</summary>
public static class ConfigFailure
{
    public const string InvalidJson = "invalid-json";
    public const string CommentsOrTrailingCommas = "comments-or-trailing-commas";
    public const string UnexpectedStructure = "unexpected-structure";
    public const string ChangedConcurrently = "changed-concurrently";
    public const string Locked = "locked";
    public const string VerifyFailed = "verify-failed";
    public const string NotInstalled = "not-installed";
    public const string Unsupported = "unsupported";
}

/// <summary>Outcome of one <see cref="JsonConfigEditor.Upsert" /> call.</summary>
public sealed record EditOutcome(bool Ok, bool AlreadyConfigured, bool Created, string Message, string? FailureReason = null, string? BackupPath = null);

/// <summary>
///     Adds or updates one server entry in a JSON client config without disturbing anything else in the file. The edit is
///     refused rather than guessed when the file cannot be rewritten faithfully (comments, trailing commas, a root that is
///     not an object). Safety steps: optimistic concurrency (the file is re-read just before the swap and the edit is
///     redone if another process changed it), a timestamped backup, an atomic replace, and a read-back that proves the
///     rest of the document is unchanged (restoring the backup if not).
/// </summary>
public static class JsonConfigEditor
{
    /// <summary>Test seam: called once per attempt, after the new content is written beside the file and before the file is re-checked, so a test can simulate another process editing it.</summary>
    public static Action<string>? BeforeCommitForTests { get; set; }

    private const int MaxAttempts = 5;
    private const int KeepBackups = 3;

    private static readonly JsonSerializerOptions s_write = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly JsonDocumentOptions s_strict = new() { CommentHandling = JsonCommentHandling.Disallow, AllowTrailingCommas = false };
    private static readonly JsonDocumentOptions s_lenient = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

    public static EditOutcome Upsert(string path, string rootKey, string entryKey, JsonObject desired, bool createIfMissing)
    {
        EditOutcome last = new(false, false, false, "Not attempted.", ConfigFailure.ChangedConcurrently);
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            last = UpsertOnce(path, rootKey, entryKey, desired, createIfMissing);
            if (last.FailureReason is not (ConfigFailure.ChangedConcurrently or ConfigFailure.Locked))
            {
                return last;
            }

            Thread.Sleep(50 * attempt);
        }

        return last;
    }

    private static EditOutcome UpsertOnce(string path, string rootKey, string entryKey, JsonObject desired, bool createIfMissing)
    {
        bool exists = File.Exists(path);
        if (!exists && !createIfMissing)
        {
            return Fail(ConfigFailure.NotInstalled, $"{path} does not exist and is not created for this client.");
        }

        byte[] original;
        try
        {
            original = exists ? ReadShared(path) : [];
        }
        catch (IOException ex)
        {
            return Fail(ConfigFailure.Locked, $"Could not read {path}: {ex.Message}");
        }

        bool hasBom = original.Length >= 3 && original[0] == 0xEF && original[1] == 0xBB && original[2] == 0xBF;
        string text = Encoding.UTF8.GetString(original, hasBom ? 3 : 0, original.Length - (hasBom ? 3 : 0));

        (JsonObject? root, EditOutcome? parseFailure) = ParseRoot(path, text);
        if (root is null)
        {
            return parseFailure!;
        }

        JsonObject? container;
        if (root[rootKey] is null)
        {
            container = new JsonObject();
            root[rootKey] = container;
        }
        else
        {
            container = root[rootKey] as JsonObject;
            if (container is null)
            {
                return Fail(ConfigFailure.UnexpectedStructure, $"'{rootKey}' in {path} is not an object.");
            }
        }

        string? existingKey = container.Select(kv => kv.Key).FirstOrDefault(k => string.Equals(k, entryKey, StringComparison.OrdinalIgnoreCase));
        if (existingKey is not null && JsonNode.DeepEquals(container[existingKey], desired))
        {
            return new EditOutcome(true, AlreadyConfigured: true, Created: false, $"Already configured in {path}.");
        }

        JsonNode? before = root.DeepClone();
        container[existingKey ?? entryKey] = desired.DeepClone();

        byte[] outBytes = Serialize(root, text, hasBom, exists);

        // In-memory proof: the new document is valid, holds the entry, and differs from the old one in that entry only.
        if (!OnlyEntryDiffers(outBytes, before, rootKey, existingKey ?? entryKey, desired))
        {
            return Fail(ConfigFailure.VerifyFailed, "The rewritten document did not match the original apart from the new entry; nothing was written.");
        }

        (string? backup, EditOutcome? commitFailure) = Commit(path, exists, original, outBytes);
        if (commitFailure is not null)
        {
            return commitFailure;
        }

        EditOutcome? readBackFailure = ReadBack(path, before, rootKey, existingKey ?? entryKey, desired, backup);
        if (readBackFailure is not null)
        {
            return readBackFailure;
        }

        return new EditOutcome(true, AlreadyConfigured: false, Created: !exists, exists ? $"Updated {path}." : $"Created {path}.", BackupPath: backup);
    }

    // Same line endings, BOM and trailing newline as the original, so the file diff is the entry (and indentation) only.
    private static byte[] Serialize(JsonObject root, string originalText, bool hasBom, bool existed)
    {
        string newline = originalText.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        // System.Text.Json writes the OS newline (CRLF on Windows): normalize to LF first, then apply the original style.
        string json = root.ToJsonString(s_write).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (newline == "\r\n")
        {
            json = json.Replace("\n", "\r\n", StringComparison.Ordinal);
        }

        string output = (hasBom ? "﻿" : "") + json + (originalText.EndsWith('\n') || !existed ? newline : "");
        return new UTF8Encoding(false).GetBytes(output);
    }

    private static (JsonObject? Root, EditOutcome? Failure) ParseRoot(string path, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return (new JsonObject(), null);
        }

        try
        {
            return JsonNode.Parse(text, nodeOptions: null, s_strict) is JsonObject parsed
                ? (parsed, null)
                : (null, Fail(ConfigFailure.UnexpectedStructure, $"{path} does not hold a JSON object at the top level."));
        }
        catch (JsonException)
        {
            return (null, TryLenient(text)
                ? Fail(ConfigFailure.CommentsOrTrailingCommas, $"{path} contains comments or trailing commas that cannot be kept when the file is rewritten; add the entry by hand (see the install prompt).")
                : Fail(ConfigFailure.InvalidJson, $"{path} is not valid JSON, so it was not modified."));
        }
    }

    // Writes the new bytes beside the file, re-checks that nobody else changed the file, keeps a backup of the original,
    // then swaps atomically. Returns the backup path, or a failure.
    private static (string? Backup, EditOutcome? Failure) Commit(string path, bool exists, byte[] original, byte[] outBytes)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        string temp = path + ".tokensaver-tmp-" + Guid.NewGuid().ToString("N")[..8];
        string? backup = null;
        try
        {
            File.WriteAllBytes(temp, outBytes);

            // Optimistic concurrency: another writer (Claude Code rewrites ~/.claude.json itself) may have changed the file
            // since we read it. If so, start over from the new content instead of overwriting their change.
            BeforeCommitForTests?.Invoke(path);
            byte[] now = File.Exists(path) ? ReadShared(path) : [];
            if (!SHA256.HashData(now).AsSpan().SequenceEqual(SHA256.HashData(original)))
            {
                File.Delete(temp);
                return (null, Fail(ConfigFailure.ChangedConcurrently, $"{path} changed while it was being edited."));
            }

            if (exists)
            {
                backup = path + ".bak-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
                File.WriteAllBytes(backup, original);
                File.Replace(temp, path, null);
                PruneBackups(path);
            }
            else
            {
                File.Move(temp, path);
            }

            return (backup, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TryDelete(temp);
            return (null, Fail(ConfigFailure.Locked, $"Could not replace {path}: {ex.Message}"));
        }
    }

    // Reads the file back and proves the rest of the document is unchanged; restores the backup if not.
    private static EditOutcome? ReadBack(string path, JsonNode? before, string rootKey, string entryKey, JsonObject desired, string? backup)
    {
        try
        {
            if (OnlyEntryDiffers(ReadShared(path), before, rootKey, entryKey, desired))
            {
                return null;
            }

            if (backup is not null)
            {
                File.Copy(backup, path, overwrite: true);
            }

            return Fail(ConfigFailure.VerifyFailed, $"{path} did not read back as expected; the original was restored.", backup);
        }
        catch (IOException ex)
        {
            return Fail(ConfigFailure.Locked, $"Wrote {path} but could not read it back: {ex.Message}", backup);
        }
    }

    private static bool OnlyEntryDiffers(byte[] bytes, JsonNode? before, string rootKey, string entryKey, JsonObject desired)
    {
        try
        {
            string t = Encoding.UTF8.GetString(bytes).TrimStart('﻿');
            if (JsonNode.Parse(t, nodeOptions: null, s_strict) is not JsonObject after)
            {
                return false;
            }

            JsonObject? afterContainer = after[rootKey] as JsonObject;
            if (afterContainer is null || !JsonNode.DeepEquals(afterContainer[entryKey], desired))
            {
                return false;
            }

            // Compare everything except the entry itself.
            JsonObject expected = (JsonObject)before!.DeepClone();
            if (expected[rootKey] is JsonObject expectedContainer)
            {
                _ = expectedContainer.Remove(entryKey);
            }

            _ = afterContainer.Remove(entryKey);

            // A root key we had to add shows up (empty) on both sides, so a plain comparison is enough.
            return JsonNode.DeepEquals(expected, after);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryLenient(string text)
    {
        try
        {
            _ = JsonNode.Parse(text, nodeOptions: null, s_lenient);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static byte[] ReadShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using MemoryStream copy = new();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private static void PruneBackups(string path)
    {
        try
        {
            string? dir = Path.GetDirectoryName(path);
            if (dir is null)
            {
                return;
            }

            foreach (string old in Directory.EnumerateFiles(dir, Path.GetFileName(path) + ".bak-*").OrderDescending().Skip(KeepBackups))
            {
                File.Delete(old);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Pruning is housekeeping; a leftover backup is harmless.
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Best effort.
        }
    }

    private static EditOutcome Fail(string reason, string message, string? backup = null) => new(false, false, false, message, reason, backup);
}
