using System.Collections.Generic;

namespace TokenSaver.Common.Models;

/// <summary>
/// Report on context hygiene and issues
/// </summary>
public class ContextHygieneReport
{
    /// <summary>
    /// Total files in context
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Total sections in context
    /// </summary>
    public int SectionCount { get; set; }

    /// <summary>
    /// Overall hygiene status
    /// </summary>
    public string Status { get; set; } = "Good";

    /// <summary>
    /// Number of issues found
    /// </summary>
    public int IssueCount { get; set; }

    /// <summary>
    /// Detected issues
    /// </summary>
    public IReadOnlyList<string> Issues { get; set; } = [];

    /// <summary>
    /// Recommendations for improvement
    /// </summary>
    public IReadOnlyList<string> Recommendations { get; set; } = [];

    /// <summary>
    /// Time taken to analyze in milliseconds
    /// </summary>
    public long AnalysisTimeMs { get; set; }
}
