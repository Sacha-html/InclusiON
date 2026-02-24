namespace InclusiON.Application.UseCases.Auth.Commands
{
    public record LoginCommand(
       string Email,
       string Password,
       bool RememberMe = false
   );
}
