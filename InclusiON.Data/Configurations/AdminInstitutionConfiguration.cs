using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class AdminInstitutionConfiguration : IEntityTypeConfiguration<AdminInstitution>
    {
        public void Configure(EntityTypeBuilder<AdminInstitution> builder)
        {
            builder.HasKey(ai => new { ai.AdminUserId, ai.InstitutionId });

            builder.HasOne(ai => ai.AdminUser)
                .WithMany(u => u.AdminInstitutions)
                .HasForeignKey(ai => ai.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ai => ai.Institution)
                .WithMany()
                .HasForeignKey(ai => ai.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
