using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivitySessionConfiguration : IEntityTypeConfiguration<ActivitySession>
    {
        public void Configure(EntityTypeBuilder<ActivitySession> builder)
        {
            builder.ToTable("ActivitySessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SuccessRate)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(s => s.DateCompleted)
                .IsRequired();

            builder.Property(s => s.ErrorCount)
                .IsRequired();

            builder.Property(s => s.TimeSpentSeconds)
                .IsRequired();

            builder.Property(s => s.GasScore)
                .IsRequired();

            builder.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Professional)
                .WithMany()
                .HasForeignKey(s => s.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Activity)
                .WithMany()
                .HasForeignKey(s => s.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.StudentId);
            builder.HasIndex(s => s.ProfessionalId);
            builder.HasIndex(s => s.ActivityId);
            builder.HasIndex(s => s.DateCompleted);
        }
    }
}
