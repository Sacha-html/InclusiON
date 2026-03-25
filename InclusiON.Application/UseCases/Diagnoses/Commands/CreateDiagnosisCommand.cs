namespace InclusiON.Application.UseCases.Diagnoses.Commands
{
    public record CreateDiagnosisCommand(
        Guid PersonId,
        Guid ProfessionalId,
        DateTime DiagnosisDate,
        string PrimaryDiagnosis,
        string? InitialObservations,
        string? IdentifiedCapabilities,
        string? IdentifiedChallenges,
        string? RequiredSupports,
        string? PedagogicalObjectives,
        string? RecommendedStrategies
    );
}
