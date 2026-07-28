namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    public record UnlockRoadmapActivityCommand(int ActivityEntryId, Guid PersonId, Guid ProfessionalId);
}
