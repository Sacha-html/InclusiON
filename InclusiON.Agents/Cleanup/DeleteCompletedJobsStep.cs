using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Agents.Cleanup;

public class DeleteCompletedJobsStep(IBackgroundJobRepository repository, ILogger<DeleteCompletedJobsStep> logger)
    : ICleanupStep
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        await repository.DeleteCompletedOlderThanAsync(cutoff, cancellationToken);
        logger.LogInformation("Deleted completed jobs older than {Cutoff}", cutoff);
    }
}
