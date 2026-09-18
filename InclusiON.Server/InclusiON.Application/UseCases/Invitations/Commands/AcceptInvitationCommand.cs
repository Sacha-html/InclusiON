namespace InclusiON.Application.UseCases.Invitations.Commands
{
    public record AcceptInvitationCommand(
        string Code,
        string Email,
        string DocumentNumber,
        string Password,
        string ConfirmPassword
    );
}
