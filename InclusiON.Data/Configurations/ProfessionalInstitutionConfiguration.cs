using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ProfessionalInstitutionConfiguration : IEntityTypeConfiguration<ProfessionalInstitution>
    {
        public void Configure(EntityTypeBuilder<ProfessionalInstitution> builder)
        {
            builder.ToTable("ProfessionalInstitutions");

            builder.HasKey(pi => new { pi.ProfessionalId, pi.InstitutionId });

            builder.Property(pi => pi.AssignedAt)
                .IsRequired();

            builder.Property(pi => pi.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(pi => pi.Professional)
                .WithMany(p => p.ProfessionalInstitutions)
                .HasForeignKey(pi => pi.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pi => pi.Institution)
                .WithMany(i => i.ProfessionalInstitutions)
                .HasForeignKey(pi => pi.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
