using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations;

public class InstitutionalRoleConfiguration : IEntityTypeConfiguration<InstitutionalRole>
{
    public void Configure(EntityTypeBuilder<InstitutionalRole> builder)
    {
        builder.ToTable("InstitutionalRoles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new InstitutionalRole { Id = 1, Name = "Director / Directora", Description = "Máxima autoridad de la institución escolar o centro", IsActive = true },
            new InstitutionalRole { Id = 2, Name = "Vicedirector / Vicedirectora", Description = "Vicedirección y gestión académica/institucional", IsActive = true },
            new InstitutionalRole { Id = 3, Name = "Secretario / Secretaria", Description = "Administración de legajos, expedientes y documentación escolar", IsActive = true },
            new InstitutionalRole { Id = 4, Name = "Preceptor / Preceptora", Description = "Acompañamiento, asistencia y coordinación de grupos escolares", IsActive = true });
    }
}
