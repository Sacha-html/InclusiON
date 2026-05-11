using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.Mappers
{
    public static class RoadmapMapper
    {
        public static RoadmapResponse ToResponse(PersonRoadmap roadmap) => new()
        {
            Id                              = roadmap.Id,
            PersonId                        = roadmap.PersonId,
            CreatedByProfessionalId         = roadmap.CreatedByProfessionalId,
            CreatedByProfessionalFullName   =
                $"{roadmap.CreatedByProfessional.FirstName} {roadmap.CreatedByProfessional.LastName}",
            Notes     = roadmap.Notes,
            CreatedAt = roadmap.CreatedAt,
            UpdatedAt = roadmap.UpdatedAt,
            Areas     = roadmap.Areas
                .OrderBy(a => a.DisplayOrder)
                .Select(ToAreaResponse)
                .ToList()
        };

        public static RoadmapAreaResponse ToAreaResponse(PersonRoadmapArea area) => new()
        {
            Id            = area.Id,
            SkillAreaId   = area.SkillAreaId,
            SkillAreaName = area.SkillArea.Name,
            Color         = area.SkillArea.Color,
            Icon          = area.SkillArea.Icon,
            DisplayOrder  = area.DisplayOrder,
            Activities    = area.Activities
                .OrderBy(a => a.SequenceOrder)
                .Select(ToActivityResponse)
                .ToList()
        };

        public static RoadmapActivityResponse ToActivityResponse(PersonRoadmapActivity act) => new()
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
