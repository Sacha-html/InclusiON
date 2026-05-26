using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations;

public class JobTypeConfiguration : IEntityTypeConfiguration<JobType>
{
    public void Configure(EntityTypeBuilder<JobType> builder)
    {
        builder.ToTable("JobTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasData(
            new JobType { Id = 1, Name = "Embedding" },
            new JobType { Id = 2, Name = "Email" },
            new JobType { Id = 3, Name = "Notificacion Push" },
            new JobType { Id = 4, Name = "Ajuste Adaptativo" },
            new JobType { Id = 5, Name = "Generacion de Templates" }
        );
    }
}
