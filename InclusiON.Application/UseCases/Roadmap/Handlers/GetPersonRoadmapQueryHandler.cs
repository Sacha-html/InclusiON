using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class GetPersonRoadmapQueryHandler
        : IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>>
    {
        private readonly IRoadmapRepository _roadmaps;

        public GetPersonRoadmapQueryHandler(IRoadmapRepository roadmaps)
        {
            _roadmaps = roadmaps;
        }

        public async Task<ApiResponse<RoadmapResponse>> HandleAsync(
            GetPersonRoadmapQuery query, CancellationToken cancellationToken)
        {
            var roadmap = await _roadmaps.GetByPersonIdAsync(query.PersonId, cancellationToken);

            if (roadmap is null)
                return ApiResponse<RoadmapResponse>.NotFound("Roadmap");

            return ApiResponse<RoadmapResponse>.SuccessResult(Map(roadmap));
        }

        internal static RoadmapResponse Map(PersonRoadmap roadmap) => new()
        {
            Id                          = roadmap.Id,
            PersonId                    = roadmap.PersonId,
            CreatedByProfessionalId     = roadmap.CreatedByProfessionalId,
            CreatedByProfessionalFullName =
                $"{roadmap.CreatedByProfessional.FirstName} {roadmap.CreatedByProfessional.LastName}",
            Notes     = roadmap.Notes,
            CreatedAt = roadmap.CreatedAt,
            UpdatedAt = roadmap.UpdatedAt,
            Areas     = roadmap.Areas
                .OrderBy(a => a.DisplayOrder)
                .Select(MapArea)
                .ToList()
        };

        private static RoadmapAreaResponse MapArea(PersonRoadmapArea area) => new()
        {
            Id            = area.Id,
            SkillAreaId   = area.SkillAreaId,
            SkillAreaName = area.SkillArea.Name,
            Color         = area.SkillArea.Color,
            Icon          = area.SkillArea.Icon,
            DisplayOrder  = area.DisplayOrder,
            Activities    = area.Activities
                .OrderBy(a => a.SequenceOrder)
                .Select(MapActivity)
                .ToList()
        };

        internal static RoadmapActivityResponse MapActivity(PersonRoadmapActivity act) => new()
        {
            Id                     = act.Id,
            ActivityId             = act.ActivityId,
            ActivityTitle          = act.Activity.Title,
            SequenceOrder          = act.SequenceOrder,
            IsUnlocked             = act.IsUnlocked,
            UnlockedAt             = act.UnlockedAt,
            UnlockThresholdPercent = act.UnlockThresholdPercent,
            TimeLimitSeconds       = act.TimeLimitSeconds,
            MaxAttempts            = act.MaxAttempts,
            ShowHints              = act.ShowHints,
            DifficultyLevel        = act.DifficultyLevel
        };
    }
}
