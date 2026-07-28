using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonRoadmapActivityConfiguration : IEntityTypeConfiguration<PersonRoadmapActivity>
    {
        public void Configure(EntityTypeBuilder<PersonRoadmapActivity> builder)
        {
            builder.ToTable("PersonRoadmapActivities");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.IsUnlocked)
                .HasDefaultValue(false);

            builder.Property(a => a.UnlockThresholdPercent)
                .HasDefaultValue(60);

            builder.Property(a => a.ShowHints)
                .HasDefaultValue(true);

            builder.Property(a => a.DifficultyLevel)
                .HasDefaultValue(1);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(a => new { a.PersonRoadmapAreaId, a.SequenceOrder })
                .IsUnique();

            builder.HasIndex(a => new { a.PersonRoadmapAreaId, a.ActivityId })
                .IsUnique();

            builder.HasIndex(a => new { a.PersonRoadmapAreaId, a.IsUnlocked });

            builder.HasOne(a => a.PersonRoadmapArea)
                .WithMany(ra => ra.Activities)
                .HasForeignKey(a => a.PersonRoadmapAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Activity)
                .WithMany(act => act.RoadmapActivities)
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
