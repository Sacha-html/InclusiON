using Microsoft.Extensions.Logging;
using InclusiON.Domain.Enums;
using InclusiON.Application.Interfaces.Repositories;

namespace InclusiON.Agents.Cleanup;

public class GenerateTemplateCentroidsStep : ICleanupStep
{
    readonly IBackgroundJobRepository _repository;
    readonly ILogger<GenerateTemplateCentroidsStep> _logger;

    public GenerateTemplateCentroidsStep(IBackgroundJobRepository repository, ILogger<GenerateTemplateCentroidsStep> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var payload = """{"triggeredBy":"midnight-cleanup"}""";
        var job = await _repository.CreateAsync(
            JobTypes.TemplateGeneration, payload,
            scheduledAt: DateTime.UtcNow, maxRetries: 3, cancellationToken);

        _logger.LogInformation("Queued TemplateGeneration job {JobId}", job.Id);
    }
}
