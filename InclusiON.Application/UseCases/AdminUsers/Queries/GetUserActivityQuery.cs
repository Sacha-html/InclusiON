namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetUserActivityQuery(Guid UserId, int Page = 1, int PageSize = 15);
}
