using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for extracting code chunks from source files
/// </summary>
public interface IChunkExtractor
{
    /// <summary>
    /// Extract chunks from a single file
    /// </summary>
    Task<IEnumerable<CodeChunk>> ExtractAsync(
        string filePath,
        string fileContent,
        CancellationToken ct = default);

    /// <summary>
    /// Extract chunks from multiple files
    /// </summary>
    Task<IEnumerable<CodeChunk>> ExtractBatchAsync(
        IEnumerable<(string Path, string Content)> files,
        CancellationToken ct = default);

    /// <summary>
    /// Supported file extensions
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }
}
