namespace InclusiON.Application.UseCases.AdminInstitutions.Commands
{
    public record AssignInstitutionToAdminCommand(Guid AdminUserId, int InstitutionId);
}
