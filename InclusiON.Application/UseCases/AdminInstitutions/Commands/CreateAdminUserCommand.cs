namespace InclusiON.Application.UseCases.AdminInstitutions.Commands
{
    public record CreateAdminUserCommand(string Email, string FirstName, string LastName, int InstitutionId);
}
