using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations;

public class BackgroundJobStatusConfiguration : IEntityTypeConfiguration<BackgroundJobStatus>
{
    public void Configure(EntityTypeBuilder<BackgroundJobStatus> builder)
    {
        builder.ToTable("BackgroundJobStatuses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.HasData(
            new BackgroundJobStatus { Id = 1, Name = "Pendiente" },
            new BackgroundJobStatus { Id = 2, Name = "En Proceso" },
            new BackgroundJobStatus { Id = 3, Name = "Completado" },
            new BackgroundJobStatus { Id = 4, Name = "Fallido" },
            new BackgroundJobStatus { Id = 5, Name = "Cancelado" }
        );
    }
}
