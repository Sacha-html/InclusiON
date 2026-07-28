namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record AddRoadmapActivityCommand(
        int AreaId,
        int ActivityId,
        Guid ProfessionalId,
        int SequenceOrder,
        int UnlockThresholdPercent,
        int? TimeLimitSeconds,
        int? MaxAttempts,
        bool ShowHints,
        int DifficultyLevel);
}
