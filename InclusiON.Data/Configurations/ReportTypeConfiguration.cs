using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ReportTypeConfiguration : IEntityTypeConfiguration<ReportType>
    {
        public void Configure(EntityTypeBuilder<ReportType> builder)
        {
            builder.ToTable("ReportTypes");

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
                new ReportType
                {
                    Id = 1,
                    Name = "Evaluación Inicial",
                    Description = "Informe de estandarización y diagnóstico inicial del estudiante",
                    IsActive = true
                },
                new ReportType
                {
                    Id = 2,
                    Name = "Seguimiento Mensual",
                    Description = "Informe de progreso mensual",
                    IsActive = true
                },
                new ReportType
                {
                    Id = 3,
                    Name = "Informe Trimestral",
                    Description = "Evaluación de progreso trimestral",
                    IsActive = true
                },
                new ReportType
                {
                    Id = 4,
                    Name = "PPI",
                    Description = "Proyecto Pedagógico Individual para la inclusión",
                    IsActive = true
                },
                new ReportType
                {
                    Id = 5,
                    Name = "Informe Anual",
                    Description = "Resumen anual de logros alcanzados y áreas a reforzar",
                    IsActive = true
                }
            );
        }
    }
}
