namespace InclusiON.DTOs.Responses.Invitations
{
    public class InvitationResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Relationship { get; set; }
        public string? PersonName { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CreatedByProfessionalName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
