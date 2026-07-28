using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class ActivityResponseConfiguration : IEntityTypeConfiguration<ActivityResponse>
    {
        public void Configure(EntityTypeBuilder<ActivityResponse> builder)
        {
            builder.ToTable("ActivityResponses");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.StartedAt)
                .IsRequired();

            builder.Property(r => r.SuccessPercentage)
                .HasColumnType("DECIMAL(5,2)");

            builder.Property(r => r.AttemptCount)
                .HasDefaultValue(1);

            builder.Property(r => r.ResponsePattern);

            builder.Property(r => r.Result)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.RequiredSupport)
                .HasDefaultValue(false);

            builder.Property(r => r.Observations);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.HasIndex(r => r.AssignmentId);

            builder.HasOne(r => r.Assignment)
                .WithMany(aa => aa.Responses)
                .HasForeignKey(r => r.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
