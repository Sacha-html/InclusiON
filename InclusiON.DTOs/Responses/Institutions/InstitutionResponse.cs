using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Institutions
{
    /// <summary>
    /// Response con los datos de una institucion educativa.
    /// </summary>
    public class InstitutionResponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static InstitutionResponse MapToResponse(EducationalInstitution institution)
        {
            return new InstitutionResponse
            {
                Id = institution.Id,
                Name = institution.Name,
                Address = institution.Address,
                Phone = institution.Phone,
                Email = institution.Email,
                IsActive = institution.IsActive,
                CreatedAt = institution.CreatedAt,
                UpdatedAt = institution.UpdatedAt
            };
        }
    }
}
