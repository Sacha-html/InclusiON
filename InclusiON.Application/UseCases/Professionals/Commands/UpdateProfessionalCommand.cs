namespace InclusiON.Application.UseCases.Professionals.Commands
{
    public record UpdateProfessionalCommand(
        Guid ProfessionalId,
        string? FirstName,
        string? LastName,
        string? DocumentNumber,
        string? Phone,
        string? Specialty,
        string? LicenseNumber,
        DateTime? BirthDate,
        string? Address
    );
}
