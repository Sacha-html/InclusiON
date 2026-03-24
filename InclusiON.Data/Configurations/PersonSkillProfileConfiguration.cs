using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class PersonSkillProfileConfiguration : IEntityTypeConfiguration<PersonSkillProfile>
    {
        public void Configure(EntityTypeBuilder<PersonSkillProfile> builder)
        {
            builder.ToTable("PersonSkillProfiles");

            builder.HasKey(psp => new { psp.PersonId, psp.SkillAreaId });

            builder.Property(psp => psp.AssignedAt)
                .IsRequired();

            builder.Property(psp => psp.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(psp => psp.Person)
                .WithMany()
                .HasForeignKey(psp => psp.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(psp => psp.SkillArea)
                .WithMany()
                .HasForeignKey(psp => psp.SkillAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
