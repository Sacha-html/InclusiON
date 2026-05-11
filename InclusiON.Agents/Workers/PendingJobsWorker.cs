using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Agents.Workers;

public class PendingJobsWorker : BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory;
    readonly IOptionsMonitor<BackgroundJobSettings> _settings;
    readonly ILogger<PendingJobsWorker> _logger;

    public PendingJobsWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BackgroundJobSettings> settings,
        ILogger<PendingJobsWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingJobsWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>();
                var executor = scope.ServiceProvider.GetRequiredService<JobExecutor>();

                var settings = _settings.CurrentValue.Worker;
                var orphanTimeout = DateTime.UtcNow.AddMinutes(-settings.OrphanTimeoutMinutes);

                var jobs = await repository.GetPendingAsync(settings.BatchSize, orphanTimeout, stoppingToken);

                if (jobs.Count > 0)
                    _logger.LogInformation("Found {Count} pending jobs", jobs.Count);

                foreach (var job in jobs)
                {
                    var claimed = await repository.TryClaimAsync(stoppingToken);
                    if (claimed is null)
                        continue;

                    await executor.ExecuteAsync(claimed, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PendingJobsWorker loop");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_settings.CurrentValue.Worker.PendingJobsIntervalSeconds),
                stoppingToken);
        }

        _logger.LogInformation("PendingJobsWorker stopped");
    }
}
