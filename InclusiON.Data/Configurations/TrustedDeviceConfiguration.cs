using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
    {
        public void Configure(EntityTypeBuilder<TrustedDevice> builder)
        {
            builder.ToTable("TrustedDevices");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd();

            builder.Property(d => d.DeviceId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.DeviceName)
                .HasMaxLength(100);

            builder.Property(d => d.DeviceType)
                .HasMaxLength(50);

            builder.Property(d => d.Browser)
                .HasMaxLength(100);

            builder.Property(d => d.OperatingSystem)
                .HasMaxLength(100);

            builder.Property(d => d.RegisteredAt)
                .IsRequired();

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(d => new { d.UserId, d.DeviceId })
                .IsUnique();

            builder.HasIndex(d => d.DeviceId);

            builder.HasOne(d => d.User)
                .WithMany(u => u.TrustedDevices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.AuthorizedByUser)
                .WithMany()
                .HasForeignKey(d => d.AuthorizedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
