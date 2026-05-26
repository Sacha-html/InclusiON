namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record CancelActivityAssignmentCommand(int AssignmentId, Guid RequestedByProfessionalId);
}
