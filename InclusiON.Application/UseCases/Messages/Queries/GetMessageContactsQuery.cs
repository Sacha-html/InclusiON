namespace InclusiON.Application.UseCases.Messages.Queries
{
    public record GetMessageContactsQuery(Guid UserId, int Page = 1, int PageSize = 100);
}
