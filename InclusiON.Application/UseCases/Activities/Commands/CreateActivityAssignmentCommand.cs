namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record CreateActivityAssignmentCommand(
        int ActivityId,
        Guid PersonId,
        Guid AssignedByProfessionalId,
        DateTime? DueDate,
        bool IsEvaluationActivity,
        int? SequenceOrder
    );
}
