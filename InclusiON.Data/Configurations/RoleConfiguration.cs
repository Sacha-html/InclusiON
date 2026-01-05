using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Enums;

namespace InclusiON.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
        {
            builder.HasData(
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = IdentityRoles.Admin.ToString(),
                    NormalizedName = IdentityRoles.Admin.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = IdentityRoles.Professional.ToString(),
                    NormalizedName = IdentityRoles.Professional.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = IdentityRoles.FamilyRepresentative.ToString(),
                    NormalizedName = IdentityRoles.FamilyRepresentative.ToString().ToUpper()
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = IdentityRoles.PersonWithDisability.ToString(),
                    NormalizedName = IdentityRoles.PersonWithDisability.ToString().ToUpper()
                }
            );
        }
    }
}
