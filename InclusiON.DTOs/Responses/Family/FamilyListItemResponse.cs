using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Family
{
    public class FamilyListItemResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{LastName}, {FirstName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
        public List<LinkedPersonInfo> LinkedPersons { get; set; } = new();

        public static FamilyListItemResponse MapToResponse(FamilyRepresentative f)
        {
            return new FamilyListItemResponse
            {
                Id = f.Id,
                UserId = f.UserId,
                FirstName = f.FirstName,
                LastName = f.LastName,
                DocumentNumber = f.DocumentNumber,
                Phone = f.Phone,
                Relationship = f.Relationship,
                IsActive = f.User?.IsActive ?? false,
                Email = f.User?.Email,
                LinkedPersons = f.PersonRepresentatives
                    .Where(pr => pr.IsActive && pr.Person != null)
                    .Select(pr => new LinkedPersonInfo
                    {
                        PersonId = pr.PersonId,
                        FullName = $"{pr.Person.FirstName} {pr.Person.LastName}".Trim(),
                        DisabilityType = pr.Person.DisabilityType?.Name,
                        IsPrimary = pr.IsPrimary
                    }).ToList()
            };
        }
    }
}
