using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Workers;

public class JobExecutor(
    IEnumerable<IJobHandler> handlers,
    IBackgroundJobRepository repository,
    ILogger<JobExecutor> logger)
{
    public async Task ExecuteAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var handler = handlers.FirstOrDefault(h => h.JobTypeId == job.JobTypeId);
        if (handler is null)
        {
            logger.LogWarning("No handler registered for JobTypeId={JobTypeId}, job {JobId}", job.JobTypeId, job.Id);
            await repository.FailAsync(job.Id, $"No handler registered for JobTypeId={job.JobTypeId}", cancellationToken);
            return;
        }

        try
        {
            logger.LogInformation("Executing job {JobId} with handler {Handler}", job.Id, handler.GetType().Name);
            await handler.HandleAsync(job, cancellationToken);
            await repository.CompleteAsync(job.Id, cancellationToken);
            logger.LogInformation("Job {JobId} completed successfully", job.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobId} failed with handler {Handler}", job.Id, handler.GetType().Name);

            if (job.RetryCount >= job.MaxRetries)
            {
                await repository.FailAsync(job.Id, ex.ToString(), cancellationToken);
                logger.LogWarning("Job {JobId} exhausted {MaxRetries} retries", job.Id, job.MaxRetries);
            }
            else
            {
                await repository.RetryAsync(job.Id, ex.ToString(), cancellationToken);
                logger.LogInformation("Job {JobId} re-queued (attempt {RetryCount}/{MaxRetries})", job.Id, job.RetryCount, job.MaxRetries);
            }
        }
    }
}
