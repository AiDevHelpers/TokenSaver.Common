using System;
using System.Collections.Generic;

namespace TokenSaver.Common.Models;

/// <summary>
/// Result from building concise context
/// </summary>
public class BuildContextResult
{
    /// <summary>
    /// The built context text
    /// </summary>
    public string ContextText { get; set; } = "";

    /// <summary>
    /// Token count of the context
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Builder version that created this result
    /// </summary>
    public string BuilderVersion { get; set; } = "1.0.0";

    /// <summary>
    /// When the context was built
    /// </summary>
    public DateTime BuiltAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the build succeeded
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Alert message if any
    /// </summary>
    public string? AlertMessage { get; set; }

    /// <summary>
    /// Source files included in context
    /// </summary>
    public int SourceFileCount { get; set; }

    /// <summary>
    /// Files excluded from context
    /// </summary>
    public int ExcludedFileCount { get; set; }

    /// <summary>
    /// Build warnings or notes
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = [];

    /// <summary>
    /// Context hygiene report
    /// </summary>
    public ContextHygieneReport? HygieneReport { get; set; }
}
