using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Agents.Cleanup;

public class GenerateTemplateCentroidsStep(
    IBackgroundJobRepository repository,
    ILogger<GenerateTemplateCentroidsStep> logger)
    : ICleanupStep
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var payload = """{"triggeredBy":"midnight-cleanup"}""";
        var job = await repository.CreateAsync(
            JobTypes.TemplateGeneration, payload,
            scheduledAt: DateTime.UtcNow, maxRetries: 3, cancellationToken);

        logger.LogInformation("Queued TemplateGeneration job {JobId}", job.Id);
    }
}
