using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
    {
        public void Configure(EntityTypeBuilder<CalendarEvent> builder)
        {
            builder.ToTable("CalendarEvents");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Date)
                .IsRequired();

            builder.Property(c => c.Time)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.StudentName)
                .HasMaxLength(200);

            builder.Property(c => c.TargetScope)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(c => c.Student)
                .WithMany()
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CreatedByProfessional)
                .WithMany()
                .HasForeignKey(c => c.CreatedByProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
