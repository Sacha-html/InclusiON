namespace InclusiON.DTOs.Responses.Admin
{
    public class AdminUserListItemResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
