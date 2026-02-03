using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
    {
        public void Configure(EntityTypeBuilder<Professional> builder)
        {
            builder.ToTable("Professionals");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.DocumentNumber)
                .HasMaxLength(20);

            builder.Property(p => p.Phone)
                .HasMaxLength(20);

            builder.Property(p => p.Specialty)
                .HasMaxLength(100);

            builder.Property(p => p.LicenseNumber)
                .HasMaxLength(50);

            builder.Property(p => p.Address)
                .HasMaxLength(255);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.HasIndex(p => p.DocumentNumber)
                .IsUnique()
                .HasFilter("[DocumentNumber] IS NOT NULL");

            builder.HasIndex(p => p.UserId)
                .IsUnique();

            // Performance indexes for professional searches
            builder.HasIndex(p => p.FirstName);
            builder.HasIndex(p => p.LastName);
            builder.HasIndex(p => p.LicenseNumber);
            builder.HasIndex(p => new { p.IsActive, p.FirstName });

            builder.HasOne(p => p.User)
                .WithOne(u => u.Professional)
                .HasForeignKey<Professional>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
