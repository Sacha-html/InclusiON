using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class DiagnosisConfiguration : IEntityTypeConfiguration<Diagnosis>
    {
        public void Configure(EntityTypeBuilder<Diagnosis> builder)
        {
            builder.ToTable("Diagnoses");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd();

            builder.Property(d => d.DiagnosisDate)
                .IsRequired();

            builder.Property(d => d.PrimaryDiagnosis)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.InitialObservations)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.IdentifiedCapabilities)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.IdentifiedChallenges)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.RequiredSupports)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.PedagogicalObjectives)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.RecommendedStrategies)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.HasIndex(d => d.PersonId);
            builder.HasIndex(d => d.ProfessionalId);

            builder.HasOne(d => d.Person)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Professional)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
