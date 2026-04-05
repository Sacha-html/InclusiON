using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Family
{
    public class FamilyStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid FamilyId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Observation { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }

        public static FamilyStatusHistoryResponse MapToResponse(FamilyStatusHistory h)
        {
            return new FamilyStatusHistoryResponse
            {
                Id = h.Id,
                FamilyId = h.FamilyId,
                OldStatus = h.OldStatus?.ToString(),
                NewStatus = h.NewStatus.ToString(),
                Observation = h.Observation,
                ChangedByUserId = h.ChangedByUserId,
                CreatedAt = h.CreatedAt
            };
        }
    }

    public class PersonRepresentativeHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public Guid RepresentativeId { get; set; }
        public string FamilyFullName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public string? Observation { get; set; }
        public DateTime CreatedAt { get; set; }

        public static PersonRepresentativeHistoryResponse MapToResponse(PersonRepresentativeHistory entity)
        {
            return new PersonRepresentativeHistoryResponse
            {
                Id = entity.Id,
                PersonId = entity.PersonId,
                RepresentativeId = entity.RepresentativeId,
                FamilyFullName = entity.Representative != null 
                    ? $"{entity.Representative.FirstName} {entity.Representative.LastName}" 
                    : string.Empty,
                Action = entity.ChangeType.ToString(),
                Relationship = entity.Relationship,
                Observation = entity.Observation,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
