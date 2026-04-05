using InclusiON.Domain.Models;

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

        public static ProfessionalListItemResponse MapToResponse(Professional p, bool includeProfessionalEmail = false)
        {
            return new ProfessionalListItemResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DocumentNumber = p.DocumentNumber,
                Phone = p.Phone,
                Specialty = p.Specialty,
                LicenseNumber = p.LicenseNumber,
                IsActive = p.User?.IsActive ?? false,
                Status = p.Status.ToString(),
                Email = includeProfessionalEmail 
                    ? (!string.IsNullOrEmpty(p.Email) ? p.Email : p.User?.Email)
                    : p.User?.Email,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
