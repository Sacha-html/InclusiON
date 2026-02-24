using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityTemplateTypeConfiguration : IEntityTypeConfiguration<ActivityTemplateType>
    {
        public void Configure(EntityTypeBuilder<ActivityTemplateType> builder)
        {
            builder.ToTable("ActivityTemplateTypes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.ContentSchema)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(t => t.ComponentName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.UsesPictograms)
                .HasDefaultValue(false);

            builder.Property(t => t.HasAudio)
                .HasDefaultValue(false);

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(t => t.Code)
                .IsUnique();

            builder.HasOne(t => t.SkillArea)
                .WithMany(s => s.TemplateTypes)
                .HasForeignKey(t => t.SkillAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
