using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Professionals
{
    public class ProfessionalStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? Observation { get; set; }
        public Guid? ChangedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public static ProfessionalStatusHistoryResponse MapToResponse(ProfessionalStatusHistory h)
        {
            return new ProfessionalStatusHistoryResponse
            {
                Id = h.Id,
                OldStatus = h.OldStatus?.ToString(),
                NewStatus = h.NewStatus.ToString(),
                Observation = h.Observation,
                ChangedByUserId = h.ChangedByUserId,
                CreatedAt = h.CreatedAt
            };
        }
    }
}
