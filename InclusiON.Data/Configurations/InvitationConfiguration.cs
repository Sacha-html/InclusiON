using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
    {
        public void Configure(EntityTypeBuilder<Invitation> builder)
        {
            builder.ToTable("Invitations");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                .ValueGeneratedOnAdd();

            builder.Property(i => i.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.FirstName)
                .HasMaxLength(100);

            builder.Property(i => i.LastName)
                .HasMaxLength(100);

            builder.Property(i => i.Relationship)
                .HasMaxLength(50);

            builder.Property(i => i.ExpiresAt)
                .IsRequired();

            builder.Property(i => i.IsUsed)
                .HasDefaultValue(false);

            builder.Property(i => i.IsActive)
                .HasDefaultValue(true);

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.HasIndex(i => i.Code)
                .IsUnique();

            builder.HasIndex(i => i.Email);

            builder.HasOne(i => i.CreatedByProfessional)
                .WithMany(p => p.CreatedInvitations)
                .HasForeignKey(i => i.CreatedByProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.ForPerson)
                .WithMany()
                .HasForeignKey(i => i.ForPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.UsedByUser)
                .WithMany()
                .HasForeignKey(i => i.UsedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
