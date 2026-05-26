namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetUserActivityQuery(Guid UserId, Guid? RequestedByUserId = null, List<int>? InstitutionIds = null, int Page = 1, int PageSize = 15);
}
