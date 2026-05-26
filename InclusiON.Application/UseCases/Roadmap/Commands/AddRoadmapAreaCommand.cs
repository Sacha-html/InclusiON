namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record AddRoadmapAreaCommand(
        Guid PersonId,
        int SkillAreaId,
        int DisplayOrder);
}
