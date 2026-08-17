namespace InclusiON.Application.UseCases.Messages.Commands
{
    public record MarkConversationReadCommand(Guid ContactUserId, Guid CurrentUserId);
}
