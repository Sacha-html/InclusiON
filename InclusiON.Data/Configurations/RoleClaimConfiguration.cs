using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations
{
    /// <summary>
    /// Configuración de permisos por rol usando AspNetRoleClaims.
    /// Los permisos se asignan a roles y se heredan automáticamente por los usuarios.
    /// </summary>
    public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
        {
            // IDs de roles (deben coincidir con RoleConfiguration.cs)
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var professionalRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var familyRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var personRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var claims = new List<IdentityRoleClaim<Guid>>
            {
                // ADMIN - Todos los permisos (IDs 1-25)
                new() { Id = 1, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:read" },
                new() { Id = 2, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:create" },
                new() { Id = 3, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:update" },
                new() { Id = 4, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "users:delete" },
                new() { Id = 5, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { Id = 6, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:create" },
                new() { Id = 7, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:update" },
                new() { Id = 8, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "persons:delete" },
                new() { Id = 9, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:read" },
                new() { Id = 10, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:create" },
                new() { Id = 11, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:update" },
                new() { Id = 12, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "professionals:delete" },
                new() { Id = 13, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:read" },
                new() { Id = 14, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:create" },
                new() { Id = 15, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:update" },
                new() { Id = 16, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "family:delete" },
                new() { Id = 17, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { Id = 18, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:create" },
                new() { Id = 19, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:update" },
                new() { Id = 20, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "activities:delete" },
                new() { Id = 21, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { Id = 22, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { Id = 23, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:create" },
                new() { Id = 24, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "reports:export" },
                new() { Id = 25, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { Id = 26, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "messages:create" },
                new() { Id = 27, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "invitations:read" },
                new() { Id = 28, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "invitations:create" },
                new() { Id = 29, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:read" },
                new() { Id = 30, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:create" },
                new() { Id = 31, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "institutions:update" },
                new() { Id = 32, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "settings:read" },
                new() { Id = 33, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "settings:update" },
                new() { Id = 34, RoleId = adminRoleId, ClaimType = "permission", ClaimValue = "audit:read" },

                // PROFESSIONAL - Gestión de personas y actividades (IDs 35-50)
                new() { Id = 35, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { Id = 36, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "persons:update" },
                new() { Id = 37, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { Id = 38, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:create" },
                new() { Id = 39, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "activities:update" },
                new() { Id = 40, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { Id = 41, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:create" },
                new() { Id = 42, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "diagnoses:update" },
                new() { Id = 43, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { Id = 44, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "reports:create" },
                new() { Id = 45, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { Id = 46, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "messages:create" },
                new() { Id = 47, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "invitations:read" },
                new() { Id = 48, RoleId = professionalRoleId, ClaimType = "permission", ClaimValue = "invitations:create" },

                // FAMILY - Ver información de sus representados (IDs 49-56)
                new() { Id = 49, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "persons:read" },
                new() { Id = 50, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { Id = 51, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "diagnoses:read" },
                new() { Id = 52, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "reports:read" },
                new() { Id = 53, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
                new() { Id = 54, RoleId = familyRoleId, ClaimType = "permission", ClaimValue = "messages:create" },

                // PERSON WITH DISABILITY - Permisos básicos (IDs 55-57)
                new() { Id = 55, RoleId = personRoleId, ClaimType = "permission", ClaimValue = "activities:read" },
                new() { Id = 56, RoleId = personRoleId, ClaimType = "permission", ClaimValue = "activities:respond" },
                new() { Id = 57, RoleId = personRoleId, ClaimType = "permission", ClaimValue = "messages:read" },
            };

            builder.HasData(claims);
        }
    }
}
