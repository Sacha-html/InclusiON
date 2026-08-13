using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ProfessionalPersonConfiguration : IEntityTypeConfiguration<ProfessionalPerson>
    {
        public void Configure(EntityTypeBuilder<ProfessionalPerson> builder)
        {
            builder.ToTable("ProfessionalPersons");

            builder.HasKey(pp => new { pp.ProfessionalId, pp.PersonId });

            builder.Property(pp => pp.AssignedAt)
                .IsRequired();

            builder.Property(pp => pp.IsPrimaryProfessional)
                .HasDefaultValue(false);

            builder.Property(pp => pp.CanSuperviseLogin)
                .HasDefaultValue(false);

            builder.Property(pp => pp.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(pp => pp.Professional)
                .WithMany(p => p.ProfessionalPersons)
                .HasForeignKey(pp => pp.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pp => pp.Person)
                .WithMany(p => p.ProfessionalPersons)
                .HasForeignKey(pp => pp.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(pp => pp.ClassroomId)
                .IsRequired(false);

            builder.HasOne(pp => pp.Classroom)
                .WithMany(c => c.ProfessionalPersons)
                .HasForeignKey(pp => pp.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
