namespace InclusiON.Domain.Models
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>SHA-256 hash del token plano enviado por email.</summary>
        public string TokenHash { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public virtual User User { get; set; } = null!;
    }
}
