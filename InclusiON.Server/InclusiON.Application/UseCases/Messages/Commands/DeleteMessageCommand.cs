namespace InclusiON.Application.UseCases.Messages.Commands
{
    public record DeleteMessageCommand(int MessageId, Guid UserId);
}
