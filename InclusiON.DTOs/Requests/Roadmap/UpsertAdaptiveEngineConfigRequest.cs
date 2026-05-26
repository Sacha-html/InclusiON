using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Roadmap;

public class UpsertAdaptiveEngineConfigRequest
{
    public bool IsEnabled { get; set; } = true;

    [Range(1, 10)] public int MinDifficultyLevel { get; set; } = 1;
    [Range(1, 10)] public int MaxDifficultyLevel { get; set; } = 5;

    [Range(0, 3600)] public int? MinTimeLimitSeconds { get; set; }
    [Range(0, 3600)] public int? MaxTimeLimitSeconds { get; set; }

    [Range(1, 20)] public int ConsecutiveSuccessToUpgrade   { get; set; } = 3;
    [Range(1, 20)] public int ConsecutiveFailuresToDowngrade { get; set; } = 2;

    [Range(1, 100)] public int SuccessThresholdPercent { get; set; } = 70;
    [Range(1, 5)]   public int FrustrationThreshold    { get; set; } = 3;
}
