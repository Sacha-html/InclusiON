namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record PatchActivityStatusCommand(
        int ActivityId,
        Guid ProfessionalId,
        bool IsActive
    );
}
