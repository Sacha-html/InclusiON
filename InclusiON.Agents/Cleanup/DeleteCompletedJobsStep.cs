using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Agents.Cleanup;

public class DeleteCompletedJobsStep : ICleanupStep
{
    readonly IBackgroundJobRepository _repository;
    readonly ILogger<DeleteCompletedJobsStep> _logger;

    public DeleteCompletedJobsStep(IBackgroundJobRepository repository, ILogger<DeleteCompletedJobsStep> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        await _repository.DeleteCompletedOlderThanAsync(cutoff, cancellationToken);
        _logger.LogInformation("Deleted completed jobs older than {Cutoff}", cutoff);
    }
}
