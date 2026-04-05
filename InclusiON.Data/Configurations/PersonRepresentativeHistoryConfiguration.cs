using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonRepresentativeHistoryConfiguration : IEntityTypeConfiguration<PersonRepresentativeHistory>
    {
        public void Configure(EntityTypeBuilder<PersonRepresentativeHistory> builder)
        {
            builder.ToTable("PersonRepresentativeHistories");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.ChangeType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(h => h.Relationship)
                .HasMaxLength(50);

            builder.Property(h => h.Observation)
                .HasMaxLength(500);

            builder.HasIndex(h => h.PersonRepresentativeId);
            builder.HasIndex(h => h.PersonId);
            builder.HasIndex(h => h.RepresentativeId);

            builder.HasOne(h => h.Person)
                .WithMany()
                .HasForeignKey(h => h.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Representative)
                .WithMany()
                .HasForeignKey(h => h.RepresentativeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
