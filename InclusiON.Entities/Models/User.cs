using Microsoft.AspNetCore.Identity;

namespace InclusiON.Entities.Models
{
    public class User : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIpAddress { get; set; }
        public string? LastLoginUserAgent { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
            RefreshTokens = new HashSet<RefreshToken>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
