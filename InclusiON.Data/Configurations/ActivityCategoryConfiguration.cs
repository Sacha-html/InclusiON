using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityCategoryConfiguration : IEntityTypeConfiguration<ActivityCategory>
    {
        public void Configure(EntityTypeBuilder<ActivityCategory> builder)
        {
            builder.ToTable("ActivityCategories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(c => c.Name)
                .IsUnique();

            // Seed Data
            builder.HasData(
                new ActivityCategory
                {
                    Id = 1,
                    Name = "Lectoescritura",
                    Description = "Actividades de lectura, escritura, conciencia fonológica y comprensión lectora",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 2,
                    Name = "Numeración y Matemática",
                    Description = "Actividades prenuméricas, numeración, secuencias y operaciones básicas",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 3,
                    Name = "Habilidades Socioemocionales",
                    Description = "Modificación de conducta, hábitos, rutinas, normas de convivencia e historias sociales",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 4,
                    Name = "Comunicación y Lenguaje",
                    Description = "Lengua de señas, sistemas aumentativos y alternativos de comunicación (SAAC)",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 5,
                    Name = "Motricidad y Coordinación",
                    Description = "Motricidad fina, motricidad gruesa, coordinación óculo-manual y orientación espacial",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 6,
                    Name = "Creatividad y Expresión Artística",
                    Description = "Música, plástica y dramatización",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 7,
                    Name = "Autonomía y Vida Diaria",
                    Description = "Vestimenta, higiene, manejo del dinero, noción del tiempo y situaciones cotidianas",
                    IsActive = true
                },
                new ActivityCategory
                {
                    Id = 8,
                    Name = "Estimulación Cognitiva",
                    Description = "Memoria, atención, percepción y resolución de problemas",
                    IsActive = true
                }
            );
        }
    }
}
