using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Agents;

public class JobExecutor
{
    readonly IEnumerable<IJobHandler> _handlers;
    readonly IBackgroundJobRepository _repository;
    readonly ILogger<JobExecutor> _logger;

    public JobExecutor(
        IEnumerable<IJobHandler> handlers,
        IBackgroundJobRepository repository,
        ILogger<JobExecutor> logger)
    {
        _handlers = handlers;
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var handler = _handlers.FirstOrDefault(h => h.JobTypeId == job.JobTypeId);
        if (handler is null)
        {
            _logger.LogWarning("No handler registered for JobTypeId={JobTypeId}, job {JobId}", job.JobTypeId, job.Id);
            await _repository.FailAsync(job.Id, $"No handler registered for JobTypeId={job.JobTypeId}", cancellationToken);
            return;
        }

        try
        {
            _logger.LogInformation("Executing job {JobId} with handler {Handler}", job.Id, handler.GetType().Name);
            await handler.HandleAsync(job, cancellationToken);
            await _repository.CompleteAsync(job.Id, cancellationToken);
            _logger.LogInformation("Job {JobId} completed successfully", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed with handler {Handler}", job.Id, handler.GetType().Name);

            if (job.RetryCount >= job.MaxRetries)
            {
                await _repository.FailAsync(job.Id, ex.ToString(), cancellationToken);
                _logger.LogWarning("Job {JobId} exhausted {MaxRetries} retries", job.Id, job.MaxRetries);
            }
            else
            {
                await _repository.FailAsync(job.Id, ex.ToString(), cancellationToken);
                _logger.LogInformation("Job {JobId} will be retried (attempt {RetryCount}/{MaxRetries})", job.Id, job.RetryCount, job.MaxRetries);
            }
        }
    }
}
