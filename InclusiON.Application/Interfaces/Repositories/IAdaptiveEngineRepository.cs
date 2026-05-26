using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.Interfaces.Repositories;

public interface IAdaptiveEngineRepository
{
    /// <summary>Gets PersonRoadmapActivity with its AdaptiveEngineConfig included.</summary>
    Task<PersonRoadmapActivity?> GetWithConfigAsync(int personRoadmapActivityId, CancellationToken ct = default);

    /// <summary>Gets last N completed ActivityResponses for a given assignment, ordered by CompletedAt desc.</summary>
    Task<List<ActivityResponse>> GetRecentResponsesByAssignmentAsync(int assignmentId, int count, CancellationToken ct = default);

    /// <summary>Saves an AdaptiveAdjustmentLog entry.</summary>
    Task AddAdjustmentLogAsync(AdaptiveAdjustmentLog log, CancellationToken ct = default);

    /// <summary>Gets adjustment history for a roadmap activity, ordered by AdjustedAt desc.</summary>
    Task<List<AdaptiveAdjustmentLog>> GetAdjustmentHistoryAsync(int personRoadmapActivityId, CancellationToken ct = default);

    /// <summary>
    /// Aggregates average success percentage per skill area from all completed
    /// activity responses belonging to the person's roadmap (IN-90).
    /// Returns one point per roadmap area; AvgSuccessPercent is null when no
    /// completed responses exist yet for that area.
    /// </summary>
    Task<List<SkillRadarPointResponse>> GetSkillRadarAsync(Guid personId, CancellationToken ct = default);

    // ── Adaptive Engine Config (IN-116) ──────────────────────────────────────

    /// <summary>Gets the adaptive engine config for a roadmap activity, or null if not configured.</summary>
    Task<AdaptiveEngineConfig?> GetConfigAsync(int personRoadmapActivityId, CancellationToken ct = default);

    /// <summary>
    /// Creates or fully replaces the adaptive engine config for the given roadmap activity.
    /// Returns the saved entity.
    /// </summary>
    Task<AdaptiveEngineConfig> UpsertConfigAsync(int personRoadmapActivityId, AdaptiveEngineConfig config, CancellationToken ct = default);

    /// <summary>Removes the adaptive engine config for the given roadmap activity (disables the motor entirely).</summary>
    Task DeleteConfigAsync(int personRoadmapActivityId, CancellationToken ct = default);
}
