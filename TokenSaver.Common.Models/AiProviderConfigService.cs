using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace AiDevTools.TokenSaver.Common.Models;

/// <summary>
///     Writes this MCP server's entry into the config of each installed AI client (VS Code Copilot, Claude Code, Cursor,
///     Continue, Cline, Sixth). Only clients that are present are touched, every write goes through
///     <see cref="JsonConfigEditor" /> (refuses what it cannot rewrite faithfully, backs up, replaces atomically, reads
///     back), and nothing runs automatically: the <c>register_mcp_providers</c> tool and <c>POST /api/register-providers</c>
///     call it on request. The known ways a client config can defeat a safe edit, and what is done about each, are in
///     docs/Host-MCP/provider-config-reliability.md.
/// </summary>
public class AiProviderConfigService
{
    /// <summary>Default MCP server key name used in all provider config files.</summary>
    public const string DefaultServerKey = "tokensaver";

    private static readonly ProviderType[] s_providers =
    [
        ProviderType.VsCodeCopilot, ProviderType.ClaudeCode, ProviderType.Cursor, ProviderType.Continue,
        ProviderType.Cline, ProviderType.Sixth, ProviderType.ClaudeDesktop,
    ];

    private readonly ProviderPaths _paths;
    private readonly Func<string?> _tokenProvider;

    public AiProviderConfigService()
        : this(ProviderPaths.Default(), () => null)
    {
    }

    /// <param name="paths">Where the clients keep their configs (a temp tree in tests).</param>
    /// <param name="tokenProvider">The endpoint bearer token to put in each entry's Authorization header; null for none.</param>
    public AiProviderConfigService(ProviderPaths paths, Func<string?> tokenProvider)
    {
        _paths = paths;
        _tokenProvider = tokenProvider;
    }

    // ── Detection ─────────────────────────────────────────────────────────────

    /// <summary>Every supported client with its config path and whether the client is installed here.</summary>
    public IReadOnlyList<DetectedProvider> DetectProviders()
    {
        return s_providers.Select(Describe).ToList();
    }

    private DetectedProvider Describe(ProviderType type)
    {
        string path = _paths.ConfigPath(type);
        bool present = File.Exists(path);
        return new DetectedProvider
        {
            Type = type,
            DisplayName = DisplayName(type),
            ConfigPath = path,
            IsConfigPresent = present,
            IsInstalled = _paths.IsInstalled(type),
            LastModified = present ? File.GetLastWriteTimeUtc(path) : null,
        };
    }

    private static string DisplayName(ProviderType type) => type switch
    {
        ProviderType.VsCodeCopilot => "VS Code (Copilot Chat)",
        ProviderType.ClaudeCode => "Claude Code",
        ProviderType.Cursor => "Cursor",
        ProviderType.Continue => "Continue",
        ProviderType.Cline => "Cline",
        ProviderType.Sixth => "Sixth AI",
        ProviderType.ClaudeDesktop => "Claude Desktop",
        _ => type.ToString(),
    };

    // ── Update ────────────────────────────────────────────────────────────────

    /// <summary>Writes the entry into every installed client. Clients that are not installed are skipped silently.</summary>
    public Task<IReadOnlyList<ConfigUpdateResult>> UpdateAllProvidersAsync(
        int mcpPort,
        string serverKey = DefaultServerKey,
        string? workspaceRoot = null,
        string? serverExePath = null,
        CancellationToken ct = default)
    {
        List<ConfigUpdateResult> results = [];
        foreach (DetectedProvider provider in DetectProviders().Where(p => p.IsInstalled))
        {
            ct.ThrowIfCancellationRequested();
            results.Add(Update(provider, mcpPort, serverKey));
        }

        return Task.FromResult<IReadOnlyList<ConfigUpdateResult>>(results);
    }

    /// <summary>Writes the entry into one client's config.</summary>
    public Task<ConfigUpdateResult> UpdateProviderAsync(
        DetectedProvider provider,
        int mcpPort,
        string serverKey = DefaultServerKey,
        string? workspaceRoot = null,
        string? serverExePath = null,
        CancellationToken ct = default)
    {
        return Task.FromResult(Update(provider, mcpPort, serverKey));
    }

    private ConfigUpdateResult Update(DetectedProvider provider, int port, string serverKey)
    {
        ProviderType type = provider.Type;
        string path = _paths.ConfigPath(type);

        if (type == ProviderType.ClaudeDesktop)
        {
            // Claude Desktop launches local servers; a remote http endpoint is added as a connector, not in this file.
            return Result(type, path, new EditOutcome(false, false, false,
                "Claude Desktop does not take an http MCP endpoint in its config file; add it as a custom connector.", ConfigFailure.Unsupported));
        }

        if (!_paths.IsInstalled(type))
        {
            return Result(type, path, new EditOutcome(false, false, false, $"{DisplayName(type)} is not installed.", ConfigFailure.NotInstalled));
        }

        string url = $"http://localhost:{port}/mcp";
        string? token = _tokenProvider();

        EditOutcome outcome = type == ProviderType.Continue
            ? WriteContinue(path, serverKey, url, token)
            : JsonConfigEditor.Upsert(
                path,
                RootKey(type),
                serverKey,
                Entry(type, url, token),
                createIfMissing: type != ProviderType.ClaudeCode);

        return Result(type, path, outcome);
    }

    private static ConfigUpdateResult Result(ProviderType type, string path, EditOutcome outcome) => new()
    {
        Provider = type,
        Success = outcome.Ok,
        WasAlreadyConfigured = outcome.AlreadyConfigured,
        ConfigPath = path,
        Message = outcome.Message,
        FailureReason = outcome.FailureReason,
    };

    private static string RootKey(ProviderType type) => type == ProviderType.VsCodeCopilot ? "servers" : "mcpServers";

    private static JsonObject Entry(ProviderType type, string url, string? token)
    {
        JsonObject entry = [];
        switch (type)
        {
            case ProviderType.VsCodeCopilot:
            case ProviderType.ClaudeCode:
                entry["type"] = "http";
                entry["url"] = url;
                break;
            case ProviderType.Cline:
                entry["type"] = "streamableHttp";
                entry["url"] = url;
                break;
            default:
                entry["url"] = url;
                break;
        }

        if (!string.IsNullOrEmpty(token))
        {
            entry["headers"] = new JsonObject { ["Authorization"] = $"Bearer {token}" };
        }

        if (type is ProviderType.Cline or ProviderType.Sixth)
        {
            entry["disabled"] = false;
        }

        return entry;
    }

    // Continue reads every file in ~/.continue/mcpServers, so TokenSaver gets a file of its own: no merging with the
    // user's config.yaml, which removes the format and concurrent-edit problems for this client.
    private static EditOutcome WriteContinue(string path, string serverKey, string url, string? token)
    {
        StringBuilder yaml = new();
        _ = yaml.Append("name: TokenSaver\nversion: 0.0.1\nschema: v1\nmcpServers:\n");
        _ = yaml.Append("  - name: ").Append(serverKey).Append('\n');
        _ = yaml.Append("    type: streamable-http\n");
        _ = yaml.Append("    url: ").Append(url).Append('\n');
        if (!string.IsNullOrEmpty(token))
        {
            _ = yaml.Append("    requestOptions:\n      headers:\n        Authorization: Bearer ").Append(token).Append('\n');
        }

        string content = yaml.ToString();
        try
        {
            if (File.Exists(path) && File.ReadAllText(path).ReplaceLineEndings("\n") == content)
            {
                return new EditOutcome(true, AlreadyConfigured: true, Created: false, $"Already configured in {path}.");
            }

            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            bool existed = File.Exists(path);
            string temp = path + ".tokensaver-tmp-" + Guid.NewGuid().ToString("N")[..8];
            File.WriteAllText(temp, content, new UTF8Encoding(false));
            if (existed)
            {
                File.Replace(temp, path, path + ".bak");
            }
            else
            {
                File.Move(temp, path);
            }

            return new EditOutcome(true, AlreadyConfigured: false, Created: !existed, existed ? $"Updated {path}." : $"Created {path}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new EditOutcome(false, false, false, $"Could not write {path}: {ex.Message}", ConfigFailure.Locked);
        }
    }
}
