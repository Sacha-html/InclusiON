namespace InclusiON.Application.UseCases.Roadmap.Commands;

public record UpsertAdaptiveEngineConfigCommand(
    int PersonRoadmapActivityId,
    bool IsEnabled,
    int MinDifficultyLevel,
    int MaxDifficultyLevel,
    int? MinTimeLimitSeconds,
    int? MaxTimeLimitSeconds,
    int ConsecutiveSuccessToUpgrade,
    int ConsecutiveFailuresToDowngrade,
    int SuccessThresholdPercent,
    int FrustrationThreshold);
