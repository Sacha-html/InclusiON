namespace InclusiON.DTOs.Responses.Professionals
{
    /// <summary>
    /// Response resumido para listados de profesionales.
    /// </summary>
    public class ProfessionalListItemResponse
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
        public bool IsActive { get; set; }
        public string? Email { get; set; } 
    }
}
