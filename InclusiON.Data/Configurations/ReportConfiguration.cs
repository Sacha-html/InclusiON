using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Content)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.ReportDate)
                .IsRequired();

            builder.Property(r => r.AchievedGoals)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.AreasToReinforce)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.FutureRecommendations)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.NextObjectives)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => r.PersonId);
            builder.HasIndex(r => r.ProfessionalId);
            builder.HasIndex(r => r.ReportTypeId);

            builder.HasOne(r => r.Person)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Professional)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReportType)
                .WithMany(t => t.Reports)
                .HasForeignKey(r => r.ReportTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
