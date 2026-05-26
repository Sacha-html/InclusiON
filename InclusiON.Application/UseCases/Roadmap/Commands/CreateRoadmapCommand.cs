namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record CreateRoadmapCommand(
        Guid PersonId,
        Guid ProfessionalId,
        string? Notes);
}
