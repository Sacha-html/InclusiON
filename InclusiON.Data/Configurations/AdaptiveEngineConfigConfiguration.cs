using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class AdaptiveEngineConfigConfiguration : IEntityTypeConfiguration<AdaptiveEngineConfig>
    {
        public void Configure(EntityTypeBuilder<AdaptiveEngineConfig> builder)
        {
            builder.ToTable("AdaptiveEngineConfigs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            // 1:0..1 con PersonRoadmapActivity
            builder.HasOne(x => x.PersonRoadmapActivity)
                .WithOne(x => x.AdaptiveConfig)
                .HasForeignKey<AdaptiveEngineConfig>(x => x.PersonRoadmapActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PersonRoadmapActivityId)
                .IsUnique();

            // Defaults
            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(true);

            builder.Property(x => x.MinDifficultyLevel)
                .HasDefaultValue(1);

            builder.Property(x => x.MaxDifficultyLevel)
                .HasDefaultValue(5);

            builder.Property(x => x.ConsecutiveSuccessToUpgrade)
                .HasDefaultValue(3);

            builder.Property(x => x.ConsecutiveFailuresToDowngrade)
                .HasDefaultValue(2);

            builder.Property(x => x.SuccessThresholdPercent)
                .HasDefaultValue(70);

            builder.Property(x => x.FrustrationThreshold)
                .HasDefaultValue(3);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);
        }
    }
}
