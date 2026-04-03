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
            // Los permisos se manejan en el DatabaseSeeder
            // Esta configuración solo mapea la entidad para EF
        }
    }
}
