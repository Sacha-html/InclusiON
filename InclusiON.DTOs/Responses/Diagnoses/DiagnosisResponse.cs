namespace InclusiON.DTOs.Responses.Diagnoses
{
    public class DiagnosisResponse
    {
        public int Id { get; set; }
        public Guid PersonId { get; set; }
        public Guid ProfessionalId { get; set; }
        public string ProfessionalName { get; set; } = string.Empty;
        public DateTime DiagnosisDate { get; set; }
        public string PrimaryDiagnosis { get; set; } = string.Empty;
        public string? InitialObservations { get; set; }
        public string? IdentifiedCapabilities { get; set; }
        public string? IdentifiedChallenges { get; set; }
        public string? RequiredSupports { get; set; }
        public string? PedagogicalObjectives { get; set; }
        public string? RecommendedStrategies { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
