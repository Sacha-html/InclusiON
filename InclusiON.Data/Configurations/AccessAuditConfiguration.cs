using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class AccessAuditConfiguration : IEntityTypeConfiguration<AccessAudit>
    {
        public void Configure(EntityTypeBuilder<AccessAudit> builder)
        {
            builder.ToTable("AccessAudits");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.ActionType)
                .IsRequired();

            builder.Property(a => a.AffectedTable)
                .HasMaxLength(100);

            builder.Property(a => a.AffectedRecordId)
                .HasMaxLength(50);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45);

            builder.Property(a => a.Timestamp)
                .IsRequired();

            builder.Property(a => a.Details);

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.AccessedPersonId);
            builder.HasIndex(a => a.Timestamp);
            builder.HasIndex(a => a.ActionType);

            builder.HasOne(a => a.User)
                .WithMany(u => u.AccessAudits)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AccessedPerson)
                .WithMany(p => p.AccessAudits)
                .HasForeignKey(a => a.AccessedPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
