namespace InclusiON.Domain.Models
{
    public class AdminInstitution
    {
        public Guid AdminUserId { get; set; }
        public int InstitutionId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual User AdminUser { get; set; } = null!;
        public virtual EducationalInstitution Institution { get; set; } = null!;
    }
}
