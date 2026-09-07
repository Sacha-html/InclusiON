namespace InclusiON.Application.UseCases.Family.Commands
{
    public record ReactivateFamilyCommand(Guid FamilyId, Guid RequestedByUserId);
}
