using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Agents.Cleanup;

public class ResetOrphanedRunningStep : ICleanupStep
{
    readonly IBackgroundJobRepository _repository;
    readonly IOptionsMonitor<BackgroundJobSettings> _settings;
    readonly ILogger<ResetOrphanedRunningStep> _logger;

    public ResetOrphanedRunningStep(
        IBackgroundJobRepository repository,
        IOptionsMonitor<BackgroundJobSettings> settings,
        ILogger<ResetOrphanedRunningStep> logger)
    {
        _repository = repository;
        _settings = settings;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var orphanTimeout = DateTime.UtcNow.AddMinutes(-_settings.CurrentValue.Worker.OrphanTimeoutMinutes);
        var count = await _repository.ResetOrphanedRunningAsync(orphanTimeout, cancellationToken);

        if (count > 0)
            _logger.LogInformation("Reset {Count} orphaned running jobs", count);
    }
}
