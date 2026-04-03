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
    }
}
