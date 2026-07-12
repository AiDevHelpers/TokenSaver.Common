using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for text distillation (summarization) services.
/// Implementations must handle null/empty inputs gracefully.
/// </summary>
public interface IDistillationService
{
    /// <summary>
    /// Distill/summarize text to reduce token count.
    /// </summary>
    /// <param name="text">Text to distill (must not be null or empty)</param>
    /// <param name="targetTokens">Target token count (must be > 0)</param>
    /// <param name="context">Optional context to preserve meaning during distillation</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Distilled result with metadata</returns>
    /// <exception cref="ArgumentNullException">text is null</exception>
    /// <exception cref="ArgumentException">text is empty or targetTokens <= 0</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task<DistillResult> DistillAsync(
        string text,
        int targetTokens,
        string? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Batch distill multiple texts efficiently.
    /// Results are returned in same order as input.
    /// </summary>
    /// <param name="texts">Texts to distill (collection must not be null)</param>
    /// <param name="targetTokensPerItem">Target tokens per item (must be > 0)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Distilled results in same order as input</returns>
    /// <exception cref="ArgumentNullException">texts is null</exception>
    /// <exception cref="ArgumentException">targetTokensPerItem <= 0 or texts contains null</exception>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task<List<DistillResult>> DistillBatchAsync(
        IEnumerable<string> texts,
        int targetTokensPerItem,
        CancellationToken ct = default);

    /// <summary>
    /// Get distillation statistics for a time period.
    /// </summary>
    /// <param name="since">Start of period (UTC); null means since service started</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Aggregate savings statistics</returns>
    /// <exception cref="OperationCanceledException">Operation cancelled</exception>
    Task<SavingsAggregate> GetSavingsAsync(
        DateTime? since = null,
        CancellationToken ct = default);
}
