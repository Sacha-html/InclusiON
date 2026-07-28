namespace InclusiON.Application.UseCases.Professionals.Commands
{
    /// <summary>
    /// Comando para el registro público de un profesional.
    /// El profesional se registra y queda en estado Pendiente de validación.
    /// </summary>
    public record RegisterProfessionalCommand(
        string FirstName,
        string LastName,
        string? DocumentNumber,
        string? Phone,
        string Specialty,
        string? LicenseNumber,
        DateTime? BirthDate,
        string Email,
        int? InstitutionId
    );
}