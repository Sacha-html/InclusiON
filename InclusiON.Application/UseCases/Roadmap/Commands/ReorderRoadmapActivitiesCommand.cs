namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record ReorderRoadmapActivitiesCommand(
        int AreaId,
        List<(int Id, int SequenceOrder)> Activities);
}
