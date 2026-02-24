using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class SkillAreaConfiguration : IEntityTypeConfiguration<SkillArea>
    {
        public void Configure(EntityTypeBuilder<SkillArea> builder)
        {
            builder.ToTable("SkillAreas");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            builder.Property(s => s.Icon)
                .HasMaxLength(50);

            builder.Property(s => s.Color)
                .HasMaxLength(7);

            builder.Property(s => s.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(s => s.Name)
                .IsUnique();

            builder.HasIndex(s => s.DisplayOrder);
        }
    }
}
