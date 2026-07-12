using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TokenSaver.Common.Models;

namespace TokenSaver.Common.Contracts;

/// <summary>
/// Contract for text distillation (summarization) services
/// </summary>
public interface IDistillationService
{
    /// <summary>
    /// Distill/summarize text to reduce token count
    /// </summary>
    Task<DistillResult> DistillAsync(
        string text,
        int targetTokens,
        string? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Batch distill multiple texts
    /// </summary>
    Task<IEnumerable<DistillResult>> DistillBatchAsync(
        IEnumerable<string> texts,
        int targetTokensPerItem,
        CancellationToken ct = default);

    /// <summary>
    /// Get distillation statistics
    /// </summary>
    Task<SavingsAggregate> GetSavingsAsync(
        DateTime? since = null,
        CancellationToken ct = default);
}
