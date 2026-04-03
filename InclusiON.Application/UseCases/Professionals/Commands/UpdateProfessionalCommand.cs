namespace InclusiON.Application.UseCases.Professionals.Commands
{
    public record UpdateProfessionalCommand(
        Guid ProfessionalId,
        string? FirstName = null,
        string? LastName = null,
        string? DocumentNumber = null,
        string? Phone = null,
        string? Specialty = null,
        string? LicenseNumber = null,
        DateTime? BirthDate = null,
        List<int>? InstitutionIds = null
    );
}
