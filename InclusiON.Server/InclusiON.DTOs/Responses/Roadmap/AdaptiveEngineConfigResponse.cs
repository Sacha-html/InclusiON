using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Roadmap;

public class AdaptiveEngineConfigResponse
{
    public int Id { get; set; }
    public int PersonRoadmapActivityId { get; set; }
    public bool IsEnabled { get; set; }
    public int MinDifficultyLevel { get; set; }
    public int MaxDifficultyLevel { get; set; }
    public int? MinTimeLimitSeconds { get; set; }
    public int? MaxTimeLimitSeconds { get; set; }
    public int ConsecutiveSuccessToUpgrade { get; set; }
    public int ConsecutiveFailuresToDowngrade { get; set; }
    public int SuccessThresholdPercent { get; set; }
    public int FrustrationThreshold { get; set; }

    public static AdaptiveEngineConfigResponse From(AdaptiveEngineConfig c) => new()
    {
        Id                            = c.Id,
        PersonRoadmapActivityId       = c.PersonRoadmapActivityId,
        IsEnabled                     = c.IsEnabled,
        MinDifficultyLevel            = c.MinDifficultyLevel,
        MaxDifficultyLevel            = c.MaxDifficultyLevel,
        MinTimeLimitSeconds           = c.MinTimeLimitSeconds,
        MaxTimeLimitSeconds           = c.MaxTimeLimitSeconds,
        ConsecutiveSuccessToUpgrade   = c.ConsecutiveSuccessToUpgrade,
        ConsecutiveFailuresToDowngrade = c.ConsecutiveFailuresToDowngrade,
        SuccessThresholdPercent       = c.SuccessThresholdPercent,
        FrustrationThreshold          = c.FrustrationThreshold,
    };
}
