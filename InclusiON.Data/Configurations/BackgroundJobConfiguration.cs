using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations;

public class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
    public void Configure(EntityTypeBuilder<BackgroundJob> builder)
    {
        builder.ToTable("BackgroundJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedOnAdd();

        builder.Property(j => j.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(j => j.RetryCount)
            .HasDefaultValue(0);

        builder.Property(j => j.MaxRetries)
            .HasDefaultValue(3);

        builder.Property(j => j.ErrorMessage)
            .HasColumnType("text");

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(j => j.JobType)
            .WithMany(t => t.BackgroundJobs)
            .HasForeignKey(j => j.JobTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Status)
            .WithMany(s => s.BackgroundJobs)
            .HasForeignKey(j => j.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(j => j.StatusId);
        builder.HasIndex(j => new { j.StatusId, j.CreatedAt });
    }
}
