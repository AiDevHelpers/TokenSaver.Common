using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for semantic search services that find relevant code chunks by meaning
/// </summary>
public interface ISemanticSearchService
{
    /// <summary>
    /// Search for code chunks semantically similar to the query
    /// </summary>
    Task<IEnumerable<SemanticSearchHit>> SearchAsync(
        string query,
        int topK = 10,
        double minRelevance = 0.5,
        CancellationToken ct = default);

    /// <summary>
    /// Index chunks for semantic search
    /// </summary>
    Task IndexChunksAsync(
        IEnumerable<CodeChunk> chunks,
        CancellationToken ct = default);

    /// <summary>
    /// Clear all indexed chunks
    /// </summary>
    Task ClearIndexAsync(CancellationToken ct = default);

    /// <summary>
    /// Get the current index size
    /// </summary>
    Task<int> GetIndexSizeAsync(CancellationToken ct = default);
}
