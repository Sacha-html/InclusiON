using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class EducationalInstitutionConfiguration : IEntityTypeConfiguration<EducationalInstitution>
    {
        public void Configure(EntityTypeBuilder<EducationalInstitution> builder)
        {
            builder.ToTable("EducationalInstitutions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(e => e.Address)
                .HasMaxLength(255);

            builder.Property(e => e.Phone)
                .HasMaxLength(20);

            builder.Property(e => e.Email)
                .HasMaxLength(100);

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .IsRequired();
        }
    }
}
