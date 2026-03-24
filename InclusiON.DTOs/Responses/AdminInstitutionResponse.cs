namespace InclusiON.DTOs.Responses
{
    public class AdminInstitutionResponse
    {
        public Guid AdminUserId { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
