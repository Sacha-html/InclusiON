namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record CreateActivityCommand(
        Guid ProfessionalId,
        string Title,
        string? Description,
        string? Instructions,
        int CategoryId,
        int? SkillAreaId,
        int? ComplexityLevel,
        int? EstimatedDurationMinutes,
        bool RequiresSupervision,
        bool HasVisualSupport,
        bool HasAudioSupport,
        bool UsesEasyReading,
        bool UsesPictograms,
        string? ResourcesUrl,
        int TemplateTypeId,
        string ContentJson
    );
}
