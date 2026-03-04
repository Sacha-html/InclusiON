using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonRepresentativeConfiguration : IEntityTypeConfiguration<PersonRepresentative>
    {
        public void Configure(EntityTypeBuilder<PersonRepresentative> builder)
        {
            builder.ToTable("PersonRepresentatives");

            builder.HasKey(pr => new { pr.PersonId, pr.RepresentativeId });

            builder.Property(pr => pr.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(pr => pr.HasInformedConsent)
                .HasDefaultValue(false);

            builder.Property(pr => pr.CanSuperviseLogin)
                .HasDefaultValue(false);

            builder.Property(pr => pr.IsActive)
                .HasDefaultValue(true);

            builder.Property(pr => pr.CreatedAt)
                .IsRequired();

            builder.HasOne(pr => pr.Person)
                .WithMany(p => p.PersonRepresentatives)
                .HasForeignKey(pr => pr.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pr => pr.Representative)
                .WithMany(r => r.PersonRepresentatives)
                .HasForeignKey(pr => pr.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
