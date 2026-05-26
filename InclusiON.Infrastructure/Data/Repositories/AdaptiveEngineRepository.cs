using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Infrastructure.Data.Repositories;

public class AdaptiveEngineRepository(AppDbContext context) : IAdaptiveEngineRepository
{
    public async Task<PersonRoadmapActivity?> GetWithConfigAsync(int personRoadmapActivityId, CancellationToken ct = default)
        => await context.PersonRoadmapActivities
            .Include(a => a.AdaptiveConfig)
            .FirstOrDefaultAsync(a => a.Id == personRoadmapActivityId, ct);

    public async Task<List<ActivityResponse>> GetRecentResponsesByAssignmentAsync(int assignmentId, int count, CancellationToken ct = default)
        => await context.ActivityResponses
            .Where(r => r.AssignmentId == assignmentId && r.CompletedAt != null)
            .OrderByDescending(r => r.CompletedAt)
            .Take(count)
            .ToListAsync(ct);

    public async Task AddAdjustmentLogAsync(AdaptiveAdjustmentLog log, CancellationToken ct = default)
    {
        context.AdaptiveAdjustmentLogs.Add(log);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<AdaptiveAdjustmentLog>> GetAdjustmentHistoryAsync(int personRoadmapActivityId, CancellationToken ct = default)
        => await context.AdaptiveAdjustmentLogs
            .Where(l => l.PersonRoadmapActivityId == personRoadmapActivityId)
            .OrderByDescending(l => l.AdjustedAt)
            .ToListAsync(ct);

    // ── Adaptive Engine Config (IN-116) ──────────────────────────────────────

    public async Task<AdaptiveEngineConfig?> GetConfigAsync(int personRoadmapActivityId, CancellationToken ct = default)
        => await context.AdaptiveEngineConfigs
            .FirstOrDefaultAsync(c => c.PersonRoadmapActivityId == personRoadmapActivityId, ct);

    public async Task<AdaptiveEngineConfig> UpsertConfigAsync(
        int personRoadmapActivityId, AdaptiveEngineConfig incoming, CancellationToken ct = default)
    {
        var existing = await context.AdaptiveEngineConfigs
            .FirstOrDefaultAsync(c => c.PersonRoadmapActivityId == personRoadmapActivityId, ct);

        if (existing is null)
        {
            incoming.PersonRoadmapActivityId = personRoadmapActivityId;
            context.AdaptiveEngineConfigs.Add(incoming);
        }
        else
        {
            existing.IsEnabled                     = incoming.IsEnabled;
            existing.MinDifficultyLevel            = incoming.MinDifficultyLevel;
            existing.MaxDifficultyLevel            = incoming.MaxDifficultyLevel;
            existing.MinTimeLimitSeconds           = incoming.MinTimeLimitSeconds;
            existing.MaxTimeLimitSeconds           = incoming.MaxTimeLimitSeconds;
            existing.ConsecutiveSuccessToUpgrade   = incoming.ConsecutiveSuccessToUpgrade;
            existing.ConsecutiveFailuresToDowngrade = incoming.ConsecutiveFailuresToDowngrade;
            existing.SuccessThresholdPercent       = incoming.SuccessThresholdPercent;
            existing.FrustrationThreshold          = incoming.FrustrationThreshold;
        }

        await context.SaveChangesAsync(ct);
        return existing ?? incoming;
    }

    public async Task DeleteConfigAsync(int personRoadmapActivityId, CancellationToken ct = default)
    {
        var config = await context.AdaptiveEngineConfigs
            .FirstOrDefaultAsync(c => c.PersonRoadmapActivityId == personRoadmapActivityId, ct);

        if (config is not null)
        {
            context.AdaptiveEngineConfigs.Remove(config);
            await context.SaveChangesAsync(ct);
        }
    }

    // ── Skill Radar (IN-90) ──────────────────────────────────────────────────

    public async Task<List<SkillRadarPointResponse>> GetSkillRadarAsync(Guid personId, CancellationToken ct = default)
    {
        // Step 1: roadmap areas with skill area metadata + activity IDs
        var areas = await context.PersonRoadmapAreas
            .Where(a => a.PersonRoadmap.PersonId == personId)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new
            {
                a.SkillArea.Name,
                a.SkillArea.Color,
                a.SkillArea.Icon,
                ActivityIds = a.Activities.Select(pa => pa.ActivityId).ToList()
            })
            .ToListAsync(ct);

        if (areas.Count == 0)
            return [];

        var allActivityIds = areas.SelectMany(a => a.ActivityIds).Distinct().ToList();

        // Step 2: completed responses per activity for this person
        var responseGroups = await context.ActivityAssignments
            .Where(aa => aa.PersonId == personId && allActivityIds.Contains(aa.ActivityId))
            .SelectMany(aa => aa.Responses
                .Where(r => r.CompletedAt != null && r.SuccessPercentage != null)
                .Select(r => new { aa.ActivityId, r.SuccessPercentage }))
            .ToListAsync(ct);

        // Step 3: avg per area (average across activities, then across responses within each activity)
        return areas.Select(a =>
        {
            var areaResponses = responseGroups
                .Where(r => a.ActivityIds.Contains(r.ActivityId))
                .ToList();

            return new SkillRadarPointResponse
            {
                AreaName       = a.Name,
                Color          = a.Color,
                Icon           = a.Icon,
                TotalResponses = areaResponses.Count,
                AvgSuccessPercent = areaResponses.Count > 0
                    ? (double)areaResponses.Average(r => r.SuccessPercentage!.Value)
                    : null
            };
        }).ToList();
    }
}
