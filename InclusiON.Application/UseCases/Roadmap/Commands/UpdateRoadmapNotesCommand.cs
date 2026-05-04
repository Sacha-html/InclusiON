namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record UpdateRoadmapNotesCommand(
        Guid PersonId,
        string? Notes);
}
