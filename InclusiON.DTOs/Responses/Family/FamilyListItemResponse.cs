namespace InclusiON.DTOs.Responses.Family
{
    public class FamilyListItemResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{LastName}, {FirstName}".Trim();
        public string? DocumentNumber { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
    }
}
