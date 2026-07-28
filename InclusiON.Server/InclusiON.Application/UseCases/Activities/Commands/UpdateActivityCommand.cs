namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record UpdateActivityCommand(
        int ActivityId,
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
        string ContentJson
    );
}
