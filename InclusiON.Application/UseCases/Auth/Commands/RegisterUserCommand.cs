using InclusiON.Domain.Enums;

namespace InclusiON.Application.UseCases.Auth.Commands
{
    public record RegisterUserCommand(
        string Name,
        string? Surname,
        string Email,
        string Password,
        string ConfirmPassword,
        string? PhoneNumber,
        IdentityRoles Role = IdentityRoles.PersonWithDisability
    );
}
