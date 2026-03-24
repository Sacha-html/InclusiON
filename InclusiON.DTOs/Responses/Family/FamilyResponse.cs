namespace InclusiON.DTOs.Responses.Family
{
    public class FamilyResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? TemporaryPassword { get; set; }
        public string? Email { get; set; }
        public List<LinkedPersonInfo>? LinkedPersons { get; set; }
    }
}
