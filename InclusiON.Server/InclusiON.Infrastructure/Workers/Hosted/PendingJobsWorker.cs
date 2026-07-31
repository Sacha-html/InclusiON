using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Workers.Hosted;

public class PendingJobsWorker(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<BackgroundJobSettings> settings,
    ILogger<PendingJobsWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PendingJobsWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>();
                var executor = scope.ServiceProvider.GetRequiredService<JobExecutor>();

                var settings1 = settings.CurrentValue.Worker;
                var orphanTimeout = DateTime.UtcNow.AddMinutes(-settings1.OrphanTimeoutMinutes);

                await repository.ResetOrphanedRunningAsync(orphanTimeout, stoppingToken);

                var processed = 0;
                for (var i = 0; i < settings1.BatchSize; i++)
                {
                    var claimed = await repository.TryClaimAsync(stoppingToken);
                    if (claimed is null)
                        break;

                    processed++;
                    await executor.ExecuteAsync(claimed, stoppingToken);
                }

                if (processed > 0)
                    logger.LogInformation("Processed {Count} jobs", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in PendingJobsWorker loop");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(settings.CurrentValue.Worker.PendingJobsIntervalSeconds),
                stoppingToken);
        }

        logger.LogInformation("PendingJobsWorker stopped");
    }
}
