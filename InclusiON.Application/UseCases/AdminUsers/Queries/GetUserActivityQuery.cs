namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetUserActivityQuery(Guid UserId, int Limit = 15);
}
