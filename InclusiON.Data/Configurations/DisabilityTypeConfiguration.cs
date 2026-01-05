using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class DisabilityTypeConfiguration : IEntityTypeConfiguration<DisabilityType>
    {
        public void Configure(EntityTypeBuilder<DisabilityType> builder)
        {
            builder.ToTable("DisabilityTypes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(t => t.Name)
                .IsUnique();

            // Seed Data
            builder.HasData(
                new DisabilityType
                {
                    Id = 1,
                    Name = "Sensorial",
                    Description = "Incluye discapacidad auditiva y visual (sordera, ceguera)",
                    IsActive = true
                },
                new DisabilityType
                {
                    Id = 2,
                    Name = "Cognitiva/Intelectual",
                    Description = "Relacionada con el aprendizaje y habilidades adaptativas de la vida diaria",
                    IsActive = true
                },
                new DisabilityType
                {
                    Id = 3,
                    Name = "Motriz",
                    Description = "Alteraciones en el funcionamiento motor",
                    IsActive = true
                },
                new DisabilityType
                {
                    Id = 4,
                    Name = "Mental/Psicosocial",
                    Description = "Derivada de diagnósticos vinculados a la salud mental",
                    IsActive = true
                },
                new DisabilityType
                {
                    Id = 5,
                    Name = "Múltiple",
                    Description = "Combinación de dos o más tipos de discapacidad",
                    IsActive = true
                }
            );
        }
    }
}
