namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record UpdateAssignmentAdaptationCommand(
        int AssignmentId,
        Guid RequestedByProfessionalId,
        DateTime? DueDate,
        int? EstimatedDurationMinutes,
        bool? HasVisualSupport,
        bool? HasAudioSupport,
        bool? UsesEasyReading,
        bool? UsesPictograms,
        bool? RequiresSupervision,
        string? CustomAdaptationNotes,
        bool AcknowledgeAlert
    );
}
