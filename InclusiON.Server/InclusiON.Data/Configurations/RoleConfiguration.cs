using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Professional",
                    NormalizedName = "PROFESSIONAL",
                    ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "FamilyRepresentative",
                    NormalizedName = "FAMILYREPRESENTATIVE",
                    ConcurrencyStamp = "33333333-3333-3333-3333-333333333333"
                },
                new IdentityRole<Guid>
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "PersonWithDisability",
                    NormalizedName = "PERSONWITHDISABILITY",
                    ConcurrencyStamp = "44444444-4444-4444-4444-444444444444"
                }
            );
        }
    }
}
