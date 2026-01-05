using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.Property(p => p.Name)
                .HasMaxLength(50);

            builder.Property(p => p.Surname)
                .HasMaxLength(50);

            builder.Property(p => p.Email)
                .HasMaxLength(100);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.MustChangePassword)
                .HasDefaultValue(false);

            builder.Property(p => p.LastLoginIpAddress)
                .HasMaxLength(45);

            builder.Property(p => p.LastLoginUserAgent)
                .HasMaxLength(500);
        }
    }
}
