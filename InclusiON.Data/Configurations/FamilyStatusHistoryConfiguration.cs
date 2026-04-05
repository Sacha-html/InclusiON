using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class FamilyStatusHistoryConfiguration : IEntityTypeConfiguration<FamilyStatusHistory>
    {
        public void Configure(EntityTypeBuilder<FamilyStatusHistory> builder)
        {
            builder.ToTable("FamilyStatusHistories");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.OldStatus)
                .HasConversion<int>();

            builder.Property(h => h.NewStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(h => h.Observation)
                .HasMaxLength(500);

            builder.HasIndex(h => h.FamilyId);

            builder.HasOne(h => h.Family)
                .WithMany(f => f.StatusHistory)
                .HasForeignKey(h => h.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
