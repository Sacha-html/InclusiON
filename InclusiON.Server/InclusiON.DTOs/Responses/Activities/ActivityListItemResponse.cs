using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Activities
{
    public class ActivityListItemResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? SkillAreaId { get; set; }
        public string? SkillAreaName { get; set; }
        public int? ComplexityLevel { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public bool IsStandardActivity { get; set; }
        public int TemplateTypeId { get; set; }
        public string TemplateTypeName { get; set; } = string.Empty;
        public string TemplateTypeCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static ActivityListItemResponse From(Activity a) => new()
        {
            Id                       = a.Id,
            Title                    = a.Title,
            Description              = a.Description,
            CategoryId               = a.CategoryId,
            CategoryName             = a.Category?.Name ?? string.Empty,
            SkillAreaId              = a.SkillAreaId,
            SkillAreaName            = a.SkillArea?.Name,
            ComplexityLevel          = a.ComplexityLevel,
            EstimatedDurationMinutes = a.EstimatedDurationMinutes,
            IsActive                 = a.IsActive,
            IsStandardActivity       = a.IsStandardActivity,
            TemplateTypeId           = a.Content?.TemplateTypeId ?? 0,
            TemplateTypeName         = a.Content?.TemplateType?.Name ?? string.Empty,
            TemplateTypeCode         = a.Content?.TemplateType?.Code ?? string.Empty,
            CreatedAt                = a.CreatedAt,
        };
    }
}
