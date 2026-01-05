using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
    {
        public void Configure(EntityTypeBuilder<Activity> builder)
        {
            builder.ToTable("Activities");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Description)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(a => a.Instructions)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(a => a.ResourcesUrl)
                .HasMaxLength(500);

            builder.Property(a => a.HasVisualSupport)
                .HasDefaultValue(false);

            builder.Property(a => a.HasAudioSupport)
                .HasDefaultValue(false);

            builder.Property(a => a.UsesEasyReading)
                .HasDefaultValue(false);

            builder.Property(a => a.UsesPictograms)
                .HasDefaultValue(false);

            builder.Property(a => a.RequiresSupervision)
                .HasDefaultValue(true);

            builder.Property(a => a.IsStandardActivity)
                .HasDefaultValue(false);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.ProfessionalId);
            builder.HasIndex(a => a.CategoryId);

            builder.HasOne(a => a.Professional)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Category)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
