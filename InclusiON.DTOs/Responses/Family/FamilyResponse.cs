using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Family
{
    public class FamilyResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? TemporaryPassword { get; set; }
        public string? Email { get; set; }
        public List<LinkedPersonInfo>? LinkedPersons { get; set; }
        public bool WasPreviouslyLinked { get; set; }

        public static FamilyResponse MapToResponse(FamilyRepresentative f, bool wasPreviouslyLinked = false)
        {
            return new FamilyResponse
            {
                Id = f.Id,
                UserId = f.UserId,
                FirstName = f.FirstName,
                LastName = f.LastName,
                DocumentNumber = f.DocumentNumber,
                Phone = f.Phone,
                Relationship = f.Relationship,
                IsActive = f.User?.IsActive ?? false,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                Email = f.User?.Email,
                WasPreviouslyLinked = wasPreviouslyLinked,
                LinkedPersons = f.PersonRepresentatives?
                    .Where(pr => pr.IsActive && pr.Person != null)
                    .Select(pr => new LinkedPersonInfo
                    {
                        PersonId = pr.PersonId,
                        FullName = $"{pr.Person!.FirstName} {pr.Person!.LastName}".Trim(),
                        DisabilityType = pr.Person.DisabilityType?.Name,
                        IsPrimary = pr.IsPrimary
                    })
                    .ToList()
            };
        }
    }
}
