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
/// Detected LLM provider configuration.
/// All properties are init-only.
/// </summary>
public class DetectedProvider
{
    public ProviderType Type { get; init; }
    public string ConfigPath { get; init; } = "";
    public bool IsInstalled { get; init; }
    public DateTime? LastModified { get; init; }
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
    public List<string> Files { get; init; } = [];
}

/// <summary>
/// Index of a conversation's structure and history.
/// The Topics collection is a copy; modifications don't affect the original.
/// </summary>
public class ConversationIndex
{
    public int TurnCount { get; init; }
    public int TotalTokens { get; init; }
    public List<string> Topics { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
