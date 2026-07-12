using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for semantic search services that find relevant code chunks by meaning.
/// Implementations may return cached results and should handle empty indexes gracefully.
/// </summary>
public interface ISemanticSearchService
{
    /// <summary>
    /// Search for code chunks semantically similar to the query.
    /// Results are ordered by relevance (descending).
    /// </summary>
    /// <param name="query">Search query (must not be null or empty)</param>
    /// <param name="topK">Maximum results to return (default 10, must be > 0)</param>
    /// <param name="minRelevance">Minimum relevance score 0.0-1.0 (default 0.5)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results ordered by relevance, or empty if no matches</returns>
    /// <exception cref="ArgumentNullException">query is null</exception>
    /// <exception cref="ArgumentException">query is empty or topK <= 0</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task<List<SemanticSearchHit>> SearchAsync(
        string query,
        int topK = 10,
        double minRelevance = 0.5,
        CancellationToken ct = default);

    /// <summary>
    /// Index chunks for future semantic searches.
    /// Replaces existing index (is not cumulative).
    /// </summary>
    /// <param name="chunks">Chunks to index (must not be null)</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="ArgumentNullException">chunks is null</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task IndexChunksAsync(
        IEnumerable<CodeChunk> chunks,
        CancellationToken ct = default);

    /// <summary>
    /// Remove all indexed chunks from the search index.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task ClearIndexAsync(CancellationToken ct = default);

    /// <summary>
    /// Get the current index size in number of chunks.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of chunks currently indexed</returns>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task<int> GetIndexSizeAsync(CancellationToken ct = default);
}
