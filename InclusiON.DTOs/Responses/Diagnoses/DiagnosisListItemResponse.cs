using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Diagnoses
{
    public class DiagnosisListItemResponse
    {
        public int Id { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public string PrimaryDiagnosis { get; set; } = string.Empty;
        public string ProfessionalName { get; set; } = string.Empty;
        public Guid ProfessionalId { get; set; }
        public DateTime CreatedAt { get; set; }

        public static DiagnosisListItemResponse MapToResponse(Diagnosis d)
        {
            return new DiagnosisListItemResponse
            {
                Id = d.Id,
                DiagnosisDate = d.DiagnosisDate,
                PrimaryDiagnosis = d.PrimaryDiagnosis,
                ProfessionalName = $"{d.Professional.FirstName} {d.Professional.LastName}".Trim(),
                ProfessionalId = d.ProfessionalId,
                CreatedAt = d.CreatedAt
            };
        }
    }
}
