using System;
using System.Collections.Generic;

namespace TokenSaver.Common.Models;

/// <summary>
/// Represents a chunk of source code.
/// All properties are init-only to prevent accidental mutation.
/// </summary>
public class CodeChunk
{
    public string Id { get; init; } = "";
    public string Path { get; init; } = "";
    public string Language { get; init; } = "";
    public string Content { get; init; } = "";
    public int StartLine { get; init; }
    public int EndLine { get; init; }
    public int TokenCount { get; init; }
    public string? Summary { get; init; }
}

/// <summary>
/// Result of distilling/summarizing text.
/// All properties are init-only to ensure immutability.
/// </summary>
public class DistillResult
{
    public string Original { get; init; } = "";
    public string Distilled { get; init; } = "";
    public double ReductionRatio { get; init; }
    public double Quality { get; init; }
    public long ExecutionTimeMs { get; init; }
}

/// <summary>
/// Statistics about a repository index.
/// All properties are init-only.
/// </summary>
public class RepoIndexStats
{
    public int FileCount { get; init; }
    public int SymbolCount { get; init; }
    public int ChunkCount { get; init; }
    public long IndexSizeBytes { get; init; }
    public DateTime LastUpdated { get; init; }
    public bool IsHealthy { get; init; }
}

/// <summary>
/// Aggregate savings metrics (tokens saved from distillation).
/// All properties are init-only.
/// </summary>
public class SavingsAggregate
{
    public long TotalOriginalTokens { get; init; }
    public long TotalDistilledTokens { get; init; }
    public long TotalSavedTokens { get; init; }
    public double ReductionRatio { get; init; }
    public int OperationCount { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Result of a semantic search query.
/// All properties are init-only.
/// </summary>
public class SemanticSearchHit
{
    public string Id { get; init; } = "";
    public string Path { get; init; } = "";
    public double Relevance { get; init; }
    public string Preview { get; init; } = "";
    public int StartLine { get; init; }
    public int EndLine { get; init; }
}

/// <summary>
/// Detected LLM provider configuration with full details.
/// All properties are init-only to prevent accidental mutation.
/// </summary>
public class DetectedProvider
{
    /// <summary>Which AI provider this is.</summary>
    public ProviderType Type { get; init; }

    /// <summary>Human-readable name for display/logging.</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    ///     Installed version, parsed from the VS Code extension folder name (e.g. "0.27.2")
    ///     or null if unavailable or not applicable.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>Absolute path to the config file that will be read/written.</summary>
    public string ConfigPath { get; init; } = "";

    /// <summary>True if the config file already exists on disk.</summary>
    public bool IsConfigPresent { get; init; }

    /// <summary>
    ///     For Continue: whether the installed version uses config.yaml (current)
    ///     or config.json (deprecated). Null for other providers.
    /// </summary>
    public ContinueConfigFormat? ContinueFormat { get; init; }

    /// <summary>True if the provider is installed on the system.</summary>
    public bool IsInstalled { get; init; }

    /// <summary>When the config file was last modified, if known.</summary>
    public DateTime? LastModified { get; init; }

    public override string ToString()
    {
        return
            $"{DisplayName}{(Version != null ? $" v{Version}" : "")} — {ConfigPath}{(IsConfigPresent ? "" : " (not yet created)")}";
    }
}

/// <summary>
/// Metadata about a section of context.
/// The Files collection is a copy; modifications don't affect the original.
/// </summary>
public class ContextSection
{
    public string Name { get; init; } = "";
    public int TokenCount { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<string> Files { get; init; } = [];
}

/// <summary>
/// Index of a conversation's structure and history.
/// The Topics collection is a copy; modifications don't affect the original.
/// </summary>
public class ConversationIndex
{
    public int TurnCount { get; init; }
    public int TotalTokens { get; init; }
    public IReadOnlyList<string> Topics { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

/// <summary>Indicates which config file format the Continue extension is using.</summary>
public enum ContinueConfigFormat
{
    /// <summary>config.yaml is present — current format.</summary>
    Yaml,

    /// <summary>Only config.json is present — deprecated format.</summary>
    Json,

    /// <summary>Neither config file is present yet.</summary>
    None
}

/// <summary>
///     Result of an MCP provider registration/configuration update.
/// </summary>
public record ConfigUpdateResult
{
    public ProviderType Provider { get; init; }

    public bool Success { get; init; }

    /// <summary>Human-readable outcome message.</summary>
    public string Message { get; init; } = "";

    /// <summary>True if the entry was already present and identical — no write was needed.</summary>
    public bool WasAlreadyConfigured { get; init; }

    /// <summary>Absolute path of the file that was written (or would have been written).</summary>
    public string? ConfigPath { get; init; }

    /// <summary>Stable code for why nothing was written (see <see cref="ConfigFailure" />), or null on success.</summary>
    public string? FailureReason { get; init; }

    public override string ToString()
    {
        return
            $"{Provider}: {(Success ? WasAlreadyConfigured ? "already configured" : "updated" : "FAILED")} — {Message}";
    }
}

/// <summary>
///     Per-workspace, per-agent user preferences persisted to ".tokensaver/preferences.json".
///     All settings are optional; absent keys fall back to adapter defaults.
/// </summary>
public sealed class WorkspacePreferences
{
    /// <summary>
    ///     Per-agent overrides. Key = agent family name (e.g. "Copilot", "Continue", "Cursor", "Anthropic").
    /// </summary>
    public Dictionary<string, AgentPreferences> Agents { get; set; } = [];
}

/// <summary>Per-agent configuration preferences.</summary>
public sealed class AgentPreferences
{
    /// <summary>
    ///     When true, send the full file content instead of a summary for files that fit within the
    ///     per-file token limit. Set this for agents that won't ask for missing context (Copilot, Continue).
    /// </summary>
    public bool? PreferFullFileContent { get; set; }

    /// <summary>Override the adapter's default per-file token limit. 0 = no limit.</summary>
    public int? MaxFileSizeTokens { get; set; }

    /// <summary>
    ///     When true, split files that exceed MaxFileSizeTokens at logical boundaries
    ///     rather than truncating them.
    /// </summary>
    public bool? SplitLargeFiles { get; set; }
}
