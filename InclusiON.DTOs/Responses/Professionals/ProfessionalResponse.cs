namespace InclusiON.DTOs.Responses.Professionals
{
    /// <summary>
    /// Response con los datos completos de un profesional.
    /// </summary>
    public class ProfessionalResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Contrasena temporal generada al crear el profesional. Solo se muestra una vez.
        /// </summary>
        public string? TemporaryPassword { get; set; }
    }
}
