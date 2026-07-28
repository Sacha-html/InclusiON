namespace InclusiON.Application.UseCases.Messages.Queries
{
    public record GetInboxQuery(
        Guid UserId,
        int Page,
        int PageSize,
        bool? IsRead = null,
        Guid? RelatedPersonId = null,
        Guid? SenderId = null);
}
