namespace InclusiON.Application.UseCases.AdminUsers.Commands
{
    public record AdminUpdateUserCommand(
        Guid UserId,
        Guid RequestedByUserId,
        string Name,
        string Surname,
        string Email);
}
