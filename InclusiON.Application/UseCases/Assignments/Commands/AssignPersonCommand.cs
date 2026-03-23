namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record AssignPersonCommand(
        Guid ProfessionalId,
        Guid PersonId,
        bool IsPrimaryProfessional,
        bool CanSuperviseLogin
    );
}
