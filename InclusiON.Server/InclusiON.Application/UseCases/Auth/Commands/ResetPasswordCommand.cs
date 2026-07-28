namespace InclusiON.Application.UseCases.Auth.Commands
{
    public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmNewPassword);
}
