namespace InclusiON.Application.UseCases.Professionals.Commands
{
    public record CreateProfessionalCommand(
        string FirstName,
        string LastName,
        string Email,
        string? DocumentNumber = null,
        string? Phone = null,
        string? Specialty = null,
        string? LicenseNumber = null,
        DateTime? BirthDate = null,
        List<int>? InstitutionIds = null
    );
}
