using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations
{
    public class ActivityResultConfiguration : IEntityTypeConfiguration<ActivityResult>
    {
        public void Configure(EntityTypeBuilder<ActivityResult> builder)
        {
            builder.ToTable("ActivityResults");

            builder.HasKey(x => x.Id);

            builder.Property(p => p.ScorePercent)
                .IsRequired();

            builder.Property(p => p.TimeSpentSeconds)
                .IsRequired();

            builder.Property(p => p.JsonResponse)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.HasOne(x => x.PersonRoadmapActivity)
                .WithMany(p => p.ActivityResults)
                .HasForeignKey(x => x.PersonRoadmapActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            //indexes
            builder.HasIndex(p => p.PersonRoadmapActivityId);
        }
    }
}
