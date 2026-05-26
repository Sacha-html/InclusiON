namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetAdminUserDetailQuery(Guid UserId, Guid? RequestedByUserId = null, List<int>? InstitutionIds = null);
}
