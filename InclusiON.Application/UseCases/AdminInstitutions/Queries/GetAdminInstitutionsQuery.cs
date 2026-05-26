namespace InclusiON.Application.UseCases.AdminInstitutions.Queries
{
    public record GetAdminInstitutionsQuery(Guid AdminUserId, int Page = 1, int PageSize = 50);
}
