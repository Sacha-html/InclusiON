namespace InclusiON.Application.UseCases.AdminUsers.Commands
{
    public record AdminDeactivateUserCommand(Guid UserId, Guid RequestedByUserId);
}
