using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
    {
        public void Configure(EntityTypeBuilder<Classroom> builder)
        {
            builder.ToTable("Classrooms");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.HasIndex(c => c.ProfessionalId);

            builder.HasOne(c => c.Professional)
                .WithMany()
                .HasForeignKey(c => c.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
