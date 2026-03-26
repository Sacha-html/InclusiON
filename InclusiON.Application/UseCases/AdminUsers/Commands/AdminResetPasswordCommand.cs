namespace InclusiON.Application.UseCases.AdminUsers.Commands
{
    public record AdminResetPasswordCommand(Guid UserId, Guid RequestedByUserId);
}
