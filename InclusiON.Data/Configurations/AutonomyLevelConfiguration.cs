using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class AutonomyLevelConfiguration : IEntityTypeConfiguration<AutonomyLevel>
    {
        public void Configure(EntityTypeBuilder<AutonomyLevel> builder)
        {
            builder.ToTable("AutonomyLevels");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Description)
                .HasMaxLength(255);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(a => a.Name)
                .IsUnique();

            // Seed Data
            builder.HasData(
                new AutonomyLevel
                {
                    Id = 1,
                    Name = "Alta",
                    Description = "Puede usar la aplicacion de forma independiente con login estandar",
                    RequiresSupervision = false,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new AutonomyLevel
                {
                    Id = 2,
                    Name = "Media",
                    Description = "Requiere login simplificado (PIN o pictogramas) pero puede usar la app solo",
                    RequiresSupervision = false,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new AutonomyLevel
                {
                    Id = 3,
                    Name = "Baja",
                    Description = "Requiere supervision y login asistido por familiar o profesional",
                    RequiresSupervision = true,
                    DisplayOrder = 3,
                    IsActive = true
                }
            );
        }
    }
}
