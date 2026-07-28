namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record CreateActivityAssignmentCommand(
        string EncryptedActivityId,
        Guid PersonId,
        Guid AssignedByProfessionalId,
        DateTime? DueDate,
        bool IsEvaluationActivity,
        int? SequenceOrder,
        bool BypassDuplicateWarning = false
    );
}
