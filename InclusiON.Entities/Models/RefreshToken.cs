using System.ComponentModel.DataAnnotations;

namespace InclusiON.Entities.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 

        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }  // Foreign Key to User

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(45)]
        public string? RevokedByIp { get; set; }

        public string? RevokedReason { get; set; }

        public virtual User User { get; set; } = null!;

        #region Audit Fields
        public string? UserAgent { get; set; }

        [MaxLength(45)]
        public string? CreatedByIp { get; set; }
        #endregion
    }
}