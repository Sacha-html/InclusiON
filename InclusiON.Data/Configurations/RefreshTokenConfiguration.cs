using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(p => p.Id);

            builder.Property(e => e.Token)
                  .HasMaxLength(512)
                  .IsRequired();

            builder.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Relación con User
            builder.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.HasIndex(e => e.Token).IsUnique();
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.ExpiresAt);
        }
    }
}
