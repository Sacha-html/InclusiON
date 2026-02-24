using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonRoadmapConfiguration : IEntityTypeConfiguration<PersonRoadmap>
    {
        public void Configure(EntityTypeBuilder<PersonRoadmap> builder)
        {
            builder.ToTable("PersonRoadmaps");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Notes)
                .HasMaxLength(1000);

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(r => r.PersonId)
                .IsUnique();

            builder.HasOne(r => r.Person)
                .WithMany()
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.CreatedByProfessional)
                .WithMany()
                .HasForeignKey(r => r.CreatedByProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
