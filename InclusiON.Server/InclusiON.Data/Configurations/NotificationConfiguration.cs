using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(n => n.EventCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.IsDismissed)
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.IsDismissed, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_UserId_IsDismissed_CreatedAt");

        builder.HasIndex(n => new { n.UserId, n.IsRead, n.IsDismissed })
            .HasDatabaseName("IX_Notifications_UserId_IsRead_IsDismissed");
    }
}
