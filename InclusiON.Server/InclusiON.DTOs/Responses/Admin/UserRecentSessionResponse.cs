namespace InclusiON.DTOs.Responses.Admin
{
    public class UserRecentSessionResponse
    {
        public DateTime CreatedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }
    }
}
