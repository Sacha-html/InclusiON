using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations
{
    public class ActivityAssignmentStatusConfiguration : IEntityTypeConfiguration<ActivityAssignmentStatus>
    {
        public void Configure(EntityTypeBuilder<ActivityAssignmentStatus> builder)
        {
            builder.ToTable("ActivityAssignmentStatuses");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(s => s.Name)
                .IsUnique();

            builder.HasData(
                new ActivityAssignmentStatus { Id = 1, Name = "Pendiente"  },
                new ActivityAssignmentStatus { Id = 2, Name = "EnProgreso" },
                new ActivityAssignmentStatus { Id = 3, Name = "Completada" },
                new ActivityAssignmentStatus { Id = 4, Name = "Cancelada"  }
            );
        }
    }
}
