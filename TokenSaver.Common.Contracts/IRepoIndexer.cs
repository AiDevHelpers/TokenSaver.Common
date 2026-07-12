using System;
using System.Threading;
using System.Threading.Tasks;
using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for indexing source code repositories
/// </summary>
public interface IRepoIndexer
{
    /// <summary>
    /// Index a repository or workspace
    /// </summary>
    Task<RepoIndexStats> IndexAsync(
        string workspacePath,
        IProgress<string>? progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get statistics about the current index
    /// </summary>
    Task<RepoIndexStats> GetStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// Clear the index
    /// </summary>
    Task ClearAsync(CancellationToken ct = default);

    /// <summary>
    /// Check if index is up to date
    /// </summary>
    Task<bool> IsUpToDateAsync(CancellationToken ct = default);

    /// <summary>
    /// Rebuild the index
    /// </summary>
    Task RebuildAsync(
        IProgress<string>? progress = null,
        CancellationToken ct = default);
}
