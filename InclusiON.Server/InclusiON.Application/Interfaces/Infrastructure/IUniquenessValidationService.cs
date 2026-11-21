namespace InclusiON.Application.Interfaces.Infrastructure
{
    public sealed record DocumentCheckResult(
        bool IsAvailable,
        string? Message = null,
        string? ConflictRole = null,
        bool IsInactiveStudent = false)
    {
        public static DocumentCheckResult Available() => new(true);

        public static DocumentCheckResult Taken(string role, bool isInactiveStudent, string message) =>
            new(false, message, role, isInactiveStudent);
    }

    public sealed record EmailCheckResult(
        bool IsAvailable,
        string? Message = null)
    {
        public static EmailCheckResult Available() => new(true);

        public static EmailCheckResult Taken(string message) => new(false, message);
    }

    public sealed record LicenseCheckResult(
        bool IsAvailable,
        string? Message = null)
    {
        public static LicenseCheckResult Available() => new(true);

        public static LicenseCheckResult Taken(string message) => new(false, message);
    }

    public static class UniquenessNormalizer
    {
        public static string? NormalizeDocument(string? documentNumber) =>
            string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim();

        public static string? NormalizeEmail(string? email) =>
            string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

        public static string? NormalizeLicense(string? licenseNumber) =>
            string.IsNullOrWhiteSpace(licenseNumber) ? null : licenseNumber.Trim();
    }

    /// <summary>
    /// Servicio centralizado de validación de unicidad global (DNI, Email y Matrícula)
    /// compartido entre endpoints de validación en vivo y CommandHandlers.
    /// </summary>
    public interface IUniquenessValidationService
    {
        Task<DocumentCheckResult> CheckDocumentAsync(
            string? documentNumber,
            Guid? excludeEntityId = null,
            CancellationToken cancellationToken = default);

        Task<EmailCheckResult> CheckEmailAsync(
            string? email,
            Guid? excludeUserId = null,
            Guid? excludeEntityId = null,
            bool checkPendingInvitations = false,
            CancellationToken cancellationToken = default);

        Task<LicenseCheckResult> CheckLicenseNumberAsync(
            string? licenseNumber,
            Guid? excludeProfessionalId = null,
            CancellationToken cancellationToken = default);
    }
}
