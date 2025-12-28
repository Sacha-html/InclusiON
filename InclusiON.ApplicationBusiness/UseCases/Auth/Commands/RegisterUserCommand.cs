namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    public record RegisterUserCommand(
        string Name,
        string? Surname,
        string Email,
        string Password,
        string ConfirmPassword,
        string? PhoneNumber
    );
}
