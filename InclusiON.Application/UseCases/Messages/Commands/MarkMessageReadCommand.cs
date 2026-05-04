namespace InclusiON.Application.UseCases.Messages.Commands
{
    public record MarkMessageReadCommand(int MessageId, Guid UserId);
}
