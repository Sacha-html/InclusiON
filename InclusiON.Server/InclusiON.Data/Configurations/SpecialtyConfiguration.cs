using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("Specialties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasData(
            new Specialty { Id = 1, Name = "Educación Especial", IsActive = true },
            new Specialty { Id = 2, Name = "Psicología", IsActive = true },
            new Specialty { Id = 3, Name = "Psicopedagogía", IsActive = true },
            new Specialty { Id = 4, Name = "Fonoaudiología", IsActive = true },
            new Specialty { Id = 5, Name = "Terapia Ocupacional", IsActive = true },
            new Specialty { Id = 6, Name = "Kinesiología", IsActive = true },
            new Specialty { Id = 7, Name = "Trabajo Social", IsActive = true },
            new Specialty { Id = 8, Name = "Musicoterapia", IsActive = true },
            new Specialty { Id = 9, Name = "Psicomotricidad", IsActive = true },
            new Specialty { Id = 10, Name = "Acompañamiento Terapéutico", IsActive = true },
            new Specialty { Id = 11, Name = "Docente de Apoyo a la Inclusión (DAI)", IsActive = true },
            new Specialty { Id = 12, Name = "Neurología", IsActive = true },
            new Specialty { Id = 13, Name = "Pediatría", IsActive = true });
    }
}
