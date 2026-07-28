namespace InclusiON.Application.UseCases.AdminInstitutions.Commands
{
    public record RemoveAdminInstitutionCommand(Guid AdminUserId, int InstitutionId);
}
