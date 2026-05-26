namespace InclusiON.Application.UseCases.Auth.Commands
{
    public record ForgotPasswordCommand(string Email, string? BaseUrl = null);
}
