namespace InclusiON.Application.UseCases.Messages.Commands
{
    public record SendMessageCommand(
        Guid SenderId,
        Guid ReceiverId,
        string? Subject,
        string Content,
        Guid? RelatedPersonId);
}
