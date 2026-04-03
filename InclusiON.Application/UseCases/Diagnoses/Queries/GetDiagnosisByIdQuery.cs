using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Application.UseCases.Diagnoses.Queries
{
    public record GetDiagnosisByIdQuery(int DiagnosisId)
    {
        internal static DiagnosisResponse MapToResponse(Diagnosis d)
        {
            return new DiagnosisResponse
            {
                Id = d.Id,
                PersonId = d.PersonId,
                ProfessionalId = d.ProfessionalId,
                ProfessionalName = $"{d.Professional.FirstName} {d.Professional.LastName}".Trim(),
                DiagnosisDate = d.DiagnosisDate,
                PrimaryDiagnosis = d.PrimaryDiagnosis,
                InitialObservations = d.InitialObservations,
                IdentifiedCapabilities = d.IdentifiedCapabilities,
                IdentifiedChallenges = d.IdentifiedChallenges,
                RequiredSupports = d.RequiredSupports,
                PedagogicalObjectives = d.PedagogicalObjectives,
                RecommendedStrategies = d.RecommendedStrategies,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            };
        }
    }
}
