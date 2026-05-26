using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers;

public class UpsertAdaptiveEngineConfigCommandHandler(
    IAdaptiveEngineRepository adaptiveRepo,
    IRoadmapRepository roadmapRepo)
    : ICommandHandler<UpsertAdaptiveEngineConfigCommand, ApiResponse<AdaptiveEngineConfigResponse>>
{
    public async Task<ApiResponse<AdaptiveEngineConfigResponse>> HandleAsync(
        UpsertAdaptiveEngineConfigCommand command, CancellationToken cancellationToken = default)
    {
        // Verify the roadmap activity exists
        var activity = await roadmapRepo.GetActivityByIdAsync(command.PersonRoadmapActivityId, cancellationToken);
        if (activity is null)
            return ApiResponse<AdaptiveEngineConfigResponse>.NotFound("Actividad del roadmap");

        var incoming = new AdaptiveEngineConfig
        {
            PersonRoadmapActivityId        = command.PersonRoadmapActivityId,
            IsEnabled                      = command.IsEnabled,
            MinDifficultyLevel             = command.MinDifficultyLevel,
            MaxDifficultyLevel             = command.MaxDifficultyLevel,
            MinTimeLimitSeconds            = command.MinTimeLimitSeconds,
            MaxTimeLimitSeconds            = command.MaxTimeLimitSeconds,
            ConsecutiveSuccessToUpgrade    = command.ConsecutiveSuccessToUpgrade,
            ConsecutiveFailuresToDowngrade = command.ConsecutiveFailuresToDowngrade,
            SuccessThresholdPercent        = command.SuccessThresholdPercent,
            FrustrationThreshold           = command.FrustrationThreshold,
        };

        var saved = await adaptiveRepo.UpsertConfigAsync(command.PersonRoadmapActivityId, incoming, cancellationToken);
        return ApiResponse<AdaptiveEngineConfigResponse>.SuccessResult(
            AdaptiveEngineConfigResponse.From(saved),
            "Configuración del motor adaptativo guardada.");
    }
}
