namespace InclusiON.Application.UseCases.Auth.Commands
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword
    );
}
