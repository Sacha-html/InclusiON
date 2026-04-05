using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Family
{
    public class PersonRepresentativeResponse
    {
        public Guid PersonId { get; set; }
        public Guid RepresentativeId { get; set; }
        public string RepresentativeFullName { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? UnlinkObservation { get; set; }

        public static PersonRepresentativeResponse MapToResponse(
            Guid personId,
            FamilyRepresentative family,
            string relationship,
            bool isPrimary,
            bool isActive = true,
            DateTime? endedAt = null,
            string? unlinkObservation = null)
        {
            return new PersonRepresentativeResponse
            {
                PersonId = personId,
                RepresentativeId = family.Id,
                RepresentativeFullName = $"{family.FirstName} {family.LastName}",
                Relationship = relationship,
                IsPrimary = isPrimary,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                EndedAt = endedAt,
                UnlinkObservation = unlinkObservation
            };
        }
    }
}
