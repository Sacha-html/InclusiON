namespace InclusiON.Application.UseCases.Institutions.Commands
{
    public record PatchInstitutionStatusCommand(int InstitutionId, bool IsActive);
}
