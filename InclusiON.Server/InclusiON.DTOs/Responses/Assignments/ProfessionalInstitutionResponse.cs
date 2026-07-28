using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Assignments
{
    /// <summary>
    /// Response con los datos de una asignacion profesional-institucion.
    /// </summary>
    public class ProfessionalInstitutionResponse
    {
        public Guid ProfessionalId { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }

        public static ProfessionalInstitutionResponse MapToResponse(ProfessionalInstitution assignment)
        {
            return new ProfessionalInstitutionResponse
            {
                ProfessionalId = assignment.ProfessionalId,
                InstitutionId = assignment.InstitutionId,
                InstitutionName = assignment.Institution?.Name ?? string.Empty,
                AssignedAt = assignment.AssignedAt,
                IsActive = assignment.IsActive
            };
        }
    }
}
