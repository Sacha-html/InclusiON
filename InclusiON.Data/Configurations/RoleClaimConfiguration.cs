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
        // IDs de roles (deben coincidir con RoleConfiguration.cs)
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ProfessionalRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid FamilyRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid PersonRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
        {
            var claims = new List<IdentityRoleClaim<Guid>>();
            var id = 1;

            // ═══════════════════════════════════════════════════════════════
            // ADMIN - Todos los permisos
            // ═══════════════════════════════════════════════════════════════
            var adminPermissions = new[]
            {
                // Usuarios
                "users:read", "users:create", "users:update", "users:delete",
                // Personas con discapacidad
                "persons:read", "persons:create", "persons:update", "persons:delete",
                // Profesionales
                "professionals:read", "professionals:create", "professionals:update", "professionals:delete",
                // Familiares
                "family:read", "family:create", "family:update", "family:delete",
                // Actividades
                "activities:read", "activities:create", "activities:update", "activities:delete",
                // Diagnósticos (solo lectura para admin)
                "diagnoses:read",
                // Reportes
                "reports:read", "reports:create", "reports:export",
                // Mensajes
                "messages:read", "messages:create",
                // Invitaciones
                "invitations:read", "invitations:create",
                // Instituciones
                "institutions:read", "institutions:create", "institutions:update",
                // Configuración
                "settings:read", "settings:update",
                // Auditoría
                "audit:read"
            };

            foreach (var permission in adminPermissions)
            {
                claims.Add(new IdentityRoleClaim<Guid>
                {
                    Id = id++,
                    RoleId = AdminRoleId,
                    ClaimType = "permission",
                    ClaimValue = permission
                });
            }

            // ═══════════════════════════════════════════════════════════════
            // PROFESSIONAL - Gestión de personas y actividades
            // ═══════════════════════════════════════════════════════════════
            var professionalPermissions = new[]
            {
                // Personas (sus asignadas)
                "persons:read", "persons:update",
                // Actividades
                "activities:read", "activities:create", "activities:update",
                // Diagnósticos
                "diagnoses:read", "diagnoses:create", "diagnoses:update",
                // Reportes
                "reports:read", "reports:create",
                // Mensajes
                "messages:read", "messages:create",
                // Invitaciones
                "invitations:read", "invitations:create"
            };

            foreach (var permission in professionalPermissions)
            {
                claims.Add(new IdentityRoleClaim<Guid>
                {
                    Id = id++,
                    RoleId = ProfessionalRoleId,
                    ClaimType = "permission",
                    ClaimValue = permission
                });
            }

            // ═══════════════════════════════════════════════════════════════
            // FAMILY - Ver información de sus representados
            // ═══════════════════════════════════════════════════════════════
            var familyPermissions = new[]
            {
                // Personas (sus representados)
                "persons:read",
                // Actividades (solo ver)
                "activities:read",
                // Diagnósticos (solo ver)
                "diagnoses:read",
                // Reportes (solo ver)
                "reports:read",
                // Mensajes
                "messages:read", "messages:create"
            };

            foreach (var permission in familyPermissions)
            {
                claims.Add(new IdentityRoleClaim<Guid>
                {
                    Id = id++,
                    RoleId = FamilyRoleId,
                    ClaimType = "permission",
                    ClaimValue = permission
                });
            }

            // ═══════════════════════════════════════════════════════════════
            // PERSON WITH DISABILITY - Permisos básicos
            // ═══════════════════════════════════════════════════════════════
            var personPermissions = new[]
            {
                // Actividades (ver y responder)
                "activities:read", "activities:respond",
                // Mensajes
                "messages:read"
            };

            foreach (var permission in personPermissions)
            {
                claims.Add(new IdentityRoleClaim<Guid>
                {
                    Id = id++,
                    RoleId = PersonRoleId,
                    ClaimType = "permission",
                    ClaimValue = permission
                });
            }

            builder.HasData(claims);
        }
    }
}
