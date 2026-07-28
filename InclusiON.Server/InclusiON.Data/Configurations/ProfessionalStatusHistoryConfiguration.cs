using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ProfessionalStatusHistoryConfiguration : IEntityTypeConfiguration<ProfessionalStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ProfessionalStatusHistory> builder)
        {
            builder.ToTable("ProfessionalStatusHistories");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.OldStatus)
                .HasConversion<int>();

            builder.Property(h => h.NewStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(h => h.Observation)
                .HasMaxLength(500);

            builder.HasIndex(h => h.ProfessionalId);

            builder.HasOne(h => h.Professional)
                .WithMany()
                .HasForeignKey(h => h.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}