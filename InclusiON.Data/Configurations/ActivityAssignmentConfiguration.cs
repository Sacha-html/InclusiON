using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityAssignmentConfiguration : IEntityTypeConfiguration<ActivityAssignment>
    {
        public void Configure(EntityTypeBuilder<ActivityAssignment> builder)
        {
            builder.ToTable("ActivityAssignments");

            builder.HasKey(aa => aa.Id);

            builder.Property(aa => aa.Id)
                .ValueGeneratedOnAdd();

            builder.Property(aa => aa.AssignedAt)
                .IsRequired();

            builder.Property(aa => aa.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente");

            builder.Property(aa => aa.IsEvaluationActivity)
                .HasDefaultValue(false);

            builder.Property(aa => aa.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(aa => aa.ActivityId);
            builder.HasIndex(aa => aa.PersonId);
            builder.HasIndex(aa => aa.AssignedByProfessionalId);
            builder.HasIndex(aa => aa.Status);

            builder.HasOne(aa => aa.Activity)
                .WithMany(a => a.ActivityAssignments)
                .HasForeignKey(aa => aa.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(aa => aa.Person)
                .WithMany(p => p.ActivityAssignments)
                .HasForeignKey(aa => aa.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(aa => aa.AssignedByProfessional)
                .WithMany(p => p.ActivityAssignments)
                .HasForeignKey(aa => aa.AssignedByProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
