namespace InclusiON.Application.UseCases.Messages.Commands
{
    public record ReplyToMessageCommand(
        Guid SenderId,
        int ParentMessageId,
        string Content);
}
