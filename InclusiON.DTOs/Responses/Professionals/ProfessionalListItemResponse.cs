namespace InclusiON.DTOs.Responses.Professionals
{
    public class ProfessionalListItemResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{LastName}, {FirstName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
