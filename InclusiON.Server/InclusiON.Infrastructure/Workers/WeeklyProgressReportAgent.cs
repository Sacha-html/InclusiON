using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

/// <summary>
/// Genera y envía un resumen semanal de progreso a cada profesional activo.
/// El resumen incluye: personas a cargo, actividades completadas en los últimos 7 días,
/// promedio de éxito por persona y alertas de frustración del período.
/// Se dispara cada domingo desde WeeklyReportCleanupStep.
/// </summary>
public class WeeklyProgressReportAgent(
    IProfessionalsRepository professionalsRepository,
    IAssignmentsRepository assignmentsRepository,
    IActivityAssignmentRepository activityAssignmentRepository,
    IBackgroundJobRepository backgroundJobs,
    IDateTimeProvider dateTime,
    ILogger<WeeklyProgressReportAgent> logger)
    : IJobHandler
{
    public int JobTypeId => JobTypes.WeeklyReport;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var periodEnd   = dateTime.UtcNow;
        var periodStart = periodEnd.AddDays(-7);

        // Obtener todos los profesionales activos
        var professionals = await professionalsRepository.GetAllActiveAsync(cancellationToken);

        if (professionals.Count == 0)
        {
            logger.LogInformation("WeeklyProgressReportAgent: no active professionals found");
            return;
        }

        logger.LogInformation(
            "WeeklyProgressReportAgent: generating weekly reports for {Count} professionals (period {Start:dd/MM} - {End:dd/MM})",
            professionals.Count, periodStart, periodEnd);

        var enqueued = 0;

        foreach (var professional in professionals)
        {
            try
            {
                var email = professional.User?.Email ?? professional.Email;
                if (string.IsNullOrWhiteSpace(email)) continue;

                // Obtener personas a cargo
                var assignments = await assignmentsRepository.GetPersonsByProfessionalIdAsync(professional.Id, cancellationToken);
                var activePersonIds = assignments.Select(a => a.PersonId).ToList();

                if (activePersonIds.Count == 0) continue;

                // Últimas respuestas de la semana (máx 50 por persona)
                var responsesByPerson = await activityAssignmentRepository
                    .GetRecentCompletedResponsesByPersonIdsAsync(activePersonIds, 50, cancellationToken);

                // Filtrar al período de la semana
                var weeklyByPerson = responsesByPerson
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.Where(r => r.CompletedAt >= periodStart).ToList());

                var totalCompleted   = weeklyByPerson.Values.Sum(r => r.Count);
                var avgSuccess       = totalCompleted > 0
                    ? weeklyByPerson.Values.SelectMany(r => r)
                        .Where(r => r.SuccessPercentage.HasValue)
                        .Select(r => r.SuccessPercentage!.Value)
                        .DefaultIfEmpty(0m)
                        .Average()
                    : 0m;

                var frustrationAlerts = weeklyByPerson.Values.SelectMany(r => r)
                    .Count(r => r.FrustrationLevel.HasValue && r.FrustrationLevel.Value >= 4);

                await backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To           = email,
                        Subject      = $"Resumen semanal InclusiON — {periodStart:dd/MM} al {periodEnd:dd/MM/yyyy}",
                        TemplateName = "WeeklyProgressReport",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "ProfessionalName",  professional.FirstName },
                            { "PeriodStart",       periodStart.ToString("dd/MM/yyyy") },
                            { "PeriodEnd",         periodEnd.ToString("dd/MM/yyyy") },
                            { "PersonCount",       activePersonIds.Count.ToString() },
                            { "TotalCompleted",    totalCompleted.ToString() },
                            { "AvgSuccess",        $"{avgSuccess:0}%" },
                            { "FrustrationAlerts", frustrationAlerts.ToString() },
                            { "Year",              periodEnd.Year.ToString() }
                        }
                    }),
                    maxRetries: 2,
                    cancellationToken: cancellationToken);

                enqueued++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "WeeklyProgressReportAgent: failed to enqueue report for professional {ProfessionalId}",
                    professional.Id);
            }
        }

        logger.LogInformation(
            "WeeklyProgressReportAgent: enqueued {Enqueued}/{Total} weekly report emails",
            enqueued, professionals.Count);
    }
}
