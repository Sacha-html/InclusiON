namespace InclusiON.Application.UseCases.Messages.Queries
{
    public record GetMessageByIdQuery(int MessageId, Guid RequestingUserId);
}
