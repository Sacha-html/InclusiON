using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonRoadmapAreaConfiguration : IEntityTypeConfiguration<PersonRoadmapArea>
    {
        public void Configure(EntityTypeBuilder<PersonRoadmapArea> builder)
        {
            builder.ToTable("PersonRoadmapAreas");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(a => new { a.PersonRoadmapId, a.SkillAreaId })
                .IsUnique();

            builder.HasOne(a => a.PersonRoadmap)
                .WithMany(r => r.Areas)
                .HasForeignKey(a => a.PersonRoadmapId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SkillArea)
                .WithMany(s => s.RoadmapAreas)
                .HasForeignKey(a => a.SkillAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
