using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class AdaptiveAdjustmentLogConfiguration : IEntityTypeConfiguration<AdaptiveAdjustmentLog>
    {
        public void Configure(EntityTypeBuilder<AdaptiveAdjustmentLog> builder)
        {
            builder.ToTable("AdaptiveAdjustmentLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.AdjustmentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PreviousValue)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(x => x.NewValue)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.AdjustedAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            // Indices
            builder.HasIndex(x => x.PersonRoadmapActivityId);
            builder.HasIndex(x => x.ActivityResponseId);
            builder.HasIndex(x => x.AdjustedAt);

            // Relaciones
            builder.HasOne(x => x.PersonRoadmapActivity)
                .WithMany(pra => pra.AdaptiveAdjustmentLogs)
                .HasForeignKey(x => x.PersonRoadmapActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ActivityResponse)
                .WithMany(ar => ar.AdaptiveAdjustmentLogs)
                .HasForeignKey(x => x.ActivityResponseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
