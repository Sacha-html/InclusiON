using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityContentConfiguration : IEntityTypeConfiguration<ActivityContent>
    {
        public void Configure(EntityTypeBuilder<ActivityContent> builder)
        {
            builder.ToTable("ActivityContents");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.ContentJson)
                .IsRequired();

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(c => c.ActivityId)
                .IsUnique();

            builder.HasOne(c => c.Activity)
                .WithOne(a => a.Content)
                .HasForeignKey<ActivityContent>(c => c.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.TemplateType)
                .WithMany(t => t.ActivityContents)
                .HasForeignKey(c => c.TemplateTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
