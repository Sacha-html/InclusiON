using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;

namespace InclusiON.Workers.Cleanup;

/// <summary>
/// Cleanup step que encola el job de reporte semanal de progreso.
/// Solo ejecuta los domingos (o si el día coincide con el configurado).
/// MidnightCleanupWorker lo invoca cada noche — la guarda del DayOfWeek
/// evita que corra el resto de los días.
/// </summary>
public class WeeklyReportCleanupStep(
    IBackgroundJobRepository repository,
    ILogger<WeeklyReportCleanupStep> logger)
    : ICleanupStep
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Solo los domingos
        if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday)
        {
            logger.LogDebug("WeeklyReportCleanupStep: skipped (not Sunday)");
            return;
        }

        var payload = """{"triggeredBy":"midnight-cleanup","scope":"all-professionals"}""";
        var job = await repository.CreateAsync(
            JobTypes.WeeklyReport, payload,
            scheduledAt: DateTime.UtcNow, maxRetries: 2, cancellationToken);

        logger.LogInformation("Queued WeeklyReport job {JobId}", job.Id);
    }
}
