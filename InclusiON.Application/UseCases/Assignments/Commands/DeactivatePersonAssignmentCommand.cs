namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record DeactivatePersonAssignmentCommand(
        Guid ProfessionalId,
        Guid PersonId
    );
}
