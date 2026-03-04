using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonWithDisabilityConfiguration : IEntityTypeConfiguration<PersonWithDisability>
    {
        public void Configure(EntityTypeBuilder<PersonWithDisability> builder)
        {
            builder.ToTable("PersonsWithDisability");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.DocumentNumber)
                .HasMaxLength(20);

            builder.Property(p => p.BirthDate)
                .IsRequired();

            builder.Property(p => p.PhotoUrl)
                .HasMaxLength(500);

            builder.Property(p => p.InterestsAndMotivators)
                .HasMaxLength(500);

            builder.Property(p => p.LearningStyle)
                .HasMaxLength(50);

            builder.Property(p => p.AvailableResources)
                .HasMaxLength(255);

            builder.Property(p => p.AdditionalTherapies)
                .HasMaxLength(500);

            // Access Configuration
            builder.Property(p => p.PinCodeHash)
                .HasMaxLength(255);

            builder.Property(p => p.EmojiSequence)
                .HasMaxLength(500);

            builder.Property(p => p.ColorShapeId);

            builder.Property(p => p.AvatarColor)
                .HasMaxLength(20);

            // Defaults
            builder.Property(p => p.UsesAAC)
                .HasDefaultValue(false);

            builder.Property(p => p.UsesSignLanguage)
                .HasDefaultValue(false);

            builder.Property(p => p.RequiresLargeFont)
                .HasDefaultValue(false);

            builder.Property(p => p.RequiresHighContrast)
                .HasDefaultValue(false);

            builder.Property(p => p.VisualNoiseSensitivity)
                .HasDefaultValue(false);

            builder.Property(p => p.SoundSensitivity)
                .HasDefaultValue(false);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(p => p.DocumentNumber)
                .IsUnique()
                .HasFilter("[DocumentNumber] IS NOT NULL");

            builder.HasIndex(p => p.UserId)
                .IsUnique();

            // Performance indexes for visual login searches
            builder.HasIndex(p => p.FirstName);
            builder.HasIndex(p => p.LastName);
            builder.HasIndex(p => p.SupervisorUserId);
            builder.HasIndex(p => p.LoginMethodId);
            builder.HasIndex(p => new { p.IsActive, p.FirstName });
            builder.HasIndex(p => new { p.IsActive, p.LastName });

            // Relationships
            builder.HasOne(p => p.User)
                .WithOne(u => u.PersonWithDisability)
                .HasForeignKey<PersonWithDisability>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.DisabilityType)
                .WithMany(t => t.PersonsWithDisability)
                .HasForeignKey(p => p.DisabilityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.AutonomyLevel)
                .WithMany(a => a.PersonsWithDisability)
                .HasForeignKey(p => p.AutonomyLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.LoginMethod)
                .WithMany(m => m.PersonsWithDisability)
                .HasForeignKey(p => p.LoginMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.SupervisorUser)
                .WithMany()
                .HasForeignKey(p => p.SupervisorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
