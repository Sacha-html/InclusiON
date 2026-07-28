namespace InclusiON.DTOs.Responses.Auth
{
    public class RefreshTokenStatsResponse
    {
        public Guid UserId { get; set; }
        public int TotalTokens { get; set; }
        public int ActiveTokens { get; set; }
        public int ExpiredTokens { get; set; }
        public int RevokedTokens { get; set; }
        public DateTime? LastTokenCreated { get; set; }
        public DateTime? OldestActiveToken { get; set; }
    }
}
