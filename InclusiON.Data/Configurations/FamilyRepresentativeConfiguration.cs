using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class FamilyRepresentativeConfiguration : IEntityTypeConfiguration<FamilyRepresentative>
    {
        public void Configure(EntityTypeBuilder<FamilyRepresentative> builder)
        {
            builder.ToTable("FamilyRepresentatives");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.DocumentNumber)
                .HasMaxLength(20);

            builder.Property(r => r.Phone)
                .HasMaxLength(20);

            builder.Property(r => r.Relationship)
                .HasMaxLength(50);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(r => r.DocumentNumber);
            builder.HasIndex(r => r.FirstName);
            builder.HasIndex(r => new { r.IsActive, r.FirstName });
            builder.HasIndex(r => r.LastName);

            builder.HasOne(r => r.User)
                .WithOne(u => u.FamilyRepresentative)
                .HasForeignKey<FamilyRepresentative>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
