namespace InclusiON.Application.UseCases.Professionals.Commands
{
    public record ReactivateProfessionalCommand(
        Guid ProfessionalId,
        string? Observation = null
    );
}
