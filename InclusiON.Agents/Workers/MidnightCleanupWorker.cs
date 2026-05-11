using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InclusiON.Agents.Cleanup;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Agents.Workers;

public class MidnightCleanupWorker : BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory;
    readonly IOptionsMonitor<BackgroundJobSettings> _settings;
    readonly ILogger<MidnightCleanupWorker> _logger;

    public MidnightCleanupWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BackgroundJobSettings> settings,
        ILogger<MidnightCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MidnightCleanupWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var targetHour = _settings.CurrentValue.Worker.MidnightCleanupHour;
            var nextRun = now.Date.AddHours(targetHour);

            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next cleanup run at {NextRun} (in {Hours}h)", nextRun, delay.TotalHours.ToString("F1"));

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var steps = scope.ServiceProvider.GetRequiredService<IEnumerable<ICleanupStep>>();

                _logger.LogInformation("Starting cleanup pipeline with {Count} steps", steps.Count());

                foreach (var step in steps)
                {
                    try
                    {
                        _logger.LogInformation("Running cleanup step: {Step}", step.GetType().Name);
                        await step.ExecuteAsync(stoppingToken);
                        _logger.LogInformation("Cleanup step completed: {Step}", step.GetType().Name);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Cleanup step failed: {Step}", step.GetType().Name);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MidnightCleanupWorker loop");
            }
        }

        _logger.LogInformation("MidnightCleanupWorker stopped");
    }
}
