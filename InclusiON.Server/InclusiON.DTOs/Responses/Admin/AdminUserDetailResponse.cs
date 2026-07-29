namespace InclusiON.DTOs.Responses.Admin
{
    public class AdminUserDetailResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool MustChangePassword { get; set; }
        public LinkedEntityInfo? LinkedEntity { get; set; }
    }

    public class LinkedEntityInfo
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
        public string? SupervisorName { get; set; }
        public string? RepresentativeName { get; set; }
    }
}
