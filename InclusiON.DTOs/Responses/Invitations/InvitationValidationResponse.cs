namespace InclusiON.DTOs.Responses.Invitations
{
    public class InvitationValidationResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Relationship { get; set; }
        public string? PersonName { get; set; }
    }
}
