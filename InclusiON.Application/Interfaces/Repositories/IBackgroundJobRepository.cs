using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories;

public interface IBackgroundJobRepository
{
    Task<BackgroundJob> CreateAsync(int jobTypeId, string payload, DateTime? scheduledAt = null, int maxRetries = 3, CancellationToken cancellationToken = default);
    Task<BackgroundJob?> TryClaimAsync(CancellationToken cancellationToken = default);
    Task CompleteAsync(int jobId, CancellationToken cancellationToken = default);
    Task FailAsync(int jobId, string errorMessage, CancellationToken cancellationToken = default);
    Task RetryAsync(int jobId, string errorMessage, CancellationToken cancellationToken = default);
    Task<List<BackgroundJob>> GetPendingAsync(int batchSize, DateTime orphanTimeout, CancellationToken cancellationToken = default);
    Task DeleteCompletedOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default);
    Task<int> ResetOrphanedRunningAsync(DateTime orphanTimeout, CancellationToken cancellationToken = default);
}
