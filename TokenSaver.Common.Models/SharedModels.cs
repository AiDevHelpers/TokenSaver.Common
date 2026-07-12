using System;
using System.Collections.Generic;

namespace TokenSaver.Common.Models;

/// <summary>
/// A piece of source code
/// </summary>
public class CodeChunk
{
    public string Id { get; set; } = "";
    public string Path { get; set; } = "";
    public string Language { get; set; } = "";
    public string Content { get; set; } = "";
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int TokenCount { get; set; }
    public string? Summary { get; set; }
}

/// <summary>
/// Result of distilling text
/// </summary>
public class DistillResult
{
    public string Original { get; set; } = "";
    public string Distilled { get; set; } = "";
    public double ReductionRatio { get; set; }
    public double Quality { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Statistics about a repository index
/// </summary>
public class RepoIndexStats
{
    public int FileCount { get; set; }
    public int SymbolCount { get; set; }
    public int ChunkCount { get; set; }
    public long IndexSizeBytes { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsHealthy { get; set; }
}

/// <summary>
/// Aggregate savings metrics
/// </summary>
public class SavingsAggregate
{
    public long TotalOriginalTokens { get; set; }
    public long TotalDistilledTokens { get; set; }
    public long TotalSavedTokens { get; set; }
    public double ReductionRatio { get; set; }
    public int OperationCount { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// Result of semantic search
/// </summary>
public class SemanticSearchHit
{
    public string Id { get; set; } = "";
    public string Path { get; set; } = "";
    public double Relevance { get; set; }
    public string Preview { get; set; } = "";
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

/// <summary>
/// Detected LLM provider for configuration
/// </summary>
public class DetectedProvider
{
    public ProviderType Type { get; set; }
    public string ConfigPath { get; set; } = "";
    public bool IsInstalled { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Context section metadata
/// </summary>
public class ContextSection
{
    public string Name { get; set; } = "";
    public int TokenCount { get; set; }
    public int FileCount { get; set; }
    public List<string> Files { get; set; } = [];
}

/// <summary>
/// Conversation history index
/// </summary>
public class ConversationIndex
{
    public int TurnCount { get; set; }
    public int TotalTokens { get; set; }
    public List<string> Topics { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
