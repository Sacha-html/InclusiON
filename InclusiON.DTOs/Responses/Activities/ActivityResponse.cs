using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Activities
{
    public class ActivityResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? SkillAreaId { get; set; }
        public string? SkillAreaName { get; set; }
        public int? ComplexityLevel { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public bool RequiresSupervision { get; set; }
        public bool HasVisualSupport { get; set; }
        public bool HasAudioSupport { get; set; }
        public bool UsesEasyReading { get; set; }
        public bool UsesPictograms { get; set; }
        public string? ResourcesUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsStandardActivity { get; set; }
        public int TemplateTypeId { get; set; }
        public string TemplateTypeName { get; set; } = string.Empty;
        public string TemplateTypeCode { get; set; } = string.Empty;
        public string? ContentSchema { get; set; }
        public string ContentJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static ActivityResponse From(Activity a) => new()
        {
            Id                       = a.Id,
            Title                    = a.Title,
            Description              = a.Description,
            Instructions             = a.Instructions,
            CategoryId               = a.CategoryId,
            CategoryName             = a.Category?.Name ?? string.Empty,
            SkillAreaId              = a.SkillAreaId,
            SkillAreaName            = a.SkillArea?.Name,
            ComplexityLevel          = a.ComplexityLevel,
            EstimatedDurationMinutes = a.EstimatedDurationMinutes,
            RequiresSupervision      = a.RequiresSupervision,
            HasVisualSupport         = a.HasVisualSupport,
            HasAudioSupport          = a.HasAudioSupport,
            UsesEasyReading          = a.UsesEasyReading,
            UsesPictograms           = a.UsesPictograms,
            ResourcesUrl             = a.ResourcesUrl,
            IsActive                 = a.IsActive,
            IsStandardActivity       = a.IsStandardActivity,
            TemplateTypeId           = a.Content?.TemplateTypeId ?? 0,
            TemplateTypeName         = a.Content?.TemplateType?.Name ?? string.Empty,
            TemplateTypeCode         = a.Content?.TemplateType?.Code ?? string.Empty,
            ContentSchema            = a.Content?.TemplateType?.ContentSchema,
            ContentJson              = a.Content?.ContentJson ?? string.Empty,
            CreatedAt                = a.CreatedAt,
            UpdatedAt                = a.UpdatedAt,
        };
    }
}
