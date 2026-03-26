namespace InclusiON.Application.UseCases.AdminUsers.Commands
{
    public record AdminReactivateUserCommand(Guid UserId, Guid RequestedByUserId);
}
