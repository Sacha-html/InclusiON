using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Workers;

public class AdaptiveAdjustmentAgent(
    IAdaptiveEngineRepository adaptiveRepo,
    IUnitOfWork unitOfWork,
    IBackgroundJobRepository backgroundJobs,
    ILogger<AdaptiveAdjustmentAgent> logger)
    : IJobHandler
{
    public int JobTypeId => JobTypes.AdaptiveAdjustment;

    public async Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Deserialize<AdaptivePayload>(job.Payload)
            ?? throw new InvalidOperationException("Invalid adaptive adjustment payload");

        var roadmapActivity = await adaptiveRepo.GetWithConfigAsync(payload.PersonRoadmapActivityId, cancellationToken);

        if (roadmapActivity is null)
        {
            logger.LogWarning("AdaptiveAdjustmentAgent: PersonRoadmapActivity {Id} not found", payload.PersonRoadmapActivityId);
            return;
        }

        var config = roadmapActivity.AdaptiveConfig;

        if (config is null || !config.IsEnabled)
        {
            logger.LogDebug("AdaptiveAdjustmentAgent: config absent or disabled for PersonRoadmapActivity {Id}", payload.PersonRoadmapActivityId);
            return;
        }

        var windowSize = config.ConsecutiveSuccessToUpgrade + 2;
        var responses = await adaptiveRepo.GetRecentResponsesByAssignmentAsync(payload.AssignmentId, windowSize, cancellationToken);

        if (responses.Count == 0)
            return;

        var latestResponse = responses[0];

        // Count consecutive successes from most recent
        int consecutiveSuccesses = 0;
        foreach (var r in responses)
        {
            if (r.Result == ActivityResponseResult.Exito
                && r.SuccessPercentage.HasValue
                && r.SuccessPercentage.Value >= config.SuccessThresholdPercent)
                consecutiveSuccesses++;
            else
                break;
        }

        // Count consecutive failures from most recent
        int consecutiveFailures = 0;
        foreach (var r in responses)
        {
            if (r.Result == ActivityResponseResult.Fallido)
                consecutiveFailures++;
            else
                break;
        }

        bool frustrationTriggered = latestResponse.FrustrationLevel.HasValue
            && latestResponse.FrustrationLevel.Value >= config.FrustrationThreshold;

        string? adjustmentType = null;
        string? reason = null;

        if (frustrationTriggered)
        {
            adjustmentType = "FrustrationIntervention";
            reason = $"Nivel de frustracion {latestResponse.FrustrationLevel} >= umbral {config.FrustrationThreshold}";
        }
        else if (consecutiveSuccesses >= config.ConsecutiveSuccessToUpgrade
                 && roadmapActivity.DifficultyLevel < config.MaxDifficultyLevel)
        {
            adjustmentType = "DifficultyUp";
            reason = $"{consecutiveSuccesses} exitos consecutivos con exito >= {config.SuccessThresholdPercent}%";
        }
        else if (consecutiveFailures >= config.ConsecutiveFailuresToDowngrade
                 && roadmapActivity.DifficultyLevel > config.MinDifficultyLevel)
        {
            adjustmentType = "DifficultyDown";
            reason = $"{consecutiveFailures} fallos consecutivos";
        }
        else
        {
            logger.LogDebug("AdaptiveAdjustmentAgent: no adjustment needed for PersonRoadmapActivity {Id}", payload.PersonRoadmapActivityId);
            return;
        }

        var previousValue = JsonSerializer.Serialize(new { DifficultyLevel = roadmapActivity.DifficultyLevel });

        if (adjustmentType == "DifficultyUp")
            roadmapActivity.DifficultyLevel = Math.Min(roadmapActivity.DifficultyLevel + 1, config.MaxDifficultyLevel);
        else
            roadmapActivity.DifficultyLevel = Math.Max(roadmapActivity.DifficultyLevel - 1, config.MinDifficultyLevel);

        var newValue = JsonSerializer.Serialize(new { DifficultyLevel = roadmapActivity.DifficultyLevel });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var log = new AdaptiveAdjustmentLog
        {
            PersonRoadmapActivityId = payload.PersonRoadmapActivityId,
            ActivityResponseId      = payload.ActivityResponseId,
            AdjustmentType          = adjustmentType,
            PreviousValue           = previousValue,
            NewValue                = newValue,
            Reason                  = reason,
            AdjustedAt              = DateTime.UtcNow
        };

        await adaptiveRepo.AddAdjustmentLogAsync(log, cancellationToken);

        logger.LogInformation(
            "AdaptiveAdjustmentAgent: {AdjustmentType} applied to PersonRoadmapActivity {Id}. {PreviousValue} -> {NewValue}. Reason: {Reason}",
            adjustmentType, payload.PersonRoadmapActivityId, previousValue, newValue, reason);

        if (adjustmentType == "FrustrationIntervention" && !string.IsNullOrEmpty(payload.ProfessionalUserId))
        {
            try
            {
                await backgroundJobs.CreateAsync(
                    JobTypes.Push,
                    JsonSerializer.Serialize(new NotificationPayload
                    {
                        UserId            = payload.ProfessionalUserId,
                        Title             = "Alerta de frustracion",
                        Message           = $"Una persona presento nivel de frustracion elevado ({latestResponse.FrustrationLevel}/5). Se redujo la dificultad automaticamente.",
                        ActionUrl         = "/#/pro/persons",
                        SendEmailFallback = false
                    }),
                    maxRetries: 3,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AdaptiveAdjustmentAgent: failed to enqueue frustration notification for professional {UserId}", payload.ProfessionalUserId);
            }
        }
    }
}

file sealed record AdaptivePayload
{
    public int PersonRoadmapActivityId { get; init; }
    public int ActivityResponseId { get; init; }
    public int AssignmentId { get; init; }
    public string ProfessionalUserId { get; init; } = string.Empty;
}
