namespace InclusiON.Application.UseCases.Messages.Queries
{
    public record GetSentQuery(
        Guid UserId,
        int Page,
        int PageSize,
        bool? IsRead = null,
        Guid? RelatedPersonId = null,
        Guid? ReceiverId = null);
}
