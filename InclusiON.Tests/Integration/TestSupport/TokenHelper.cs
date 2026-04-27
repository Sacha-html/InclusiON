using System.Security.Claims;
using System.Text;
using InclusiON.Application.Constants;
using InclusiON.Domain.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InclusiON.Tests.Integration.TestSupport
{
    /// <summary>
    /// Genera tokens JWT firmados con la clave de test para inyectar en requests HTTP de integración.
    /// </summary>
    internal static class TokenHelper
    {
        private const string Secret   = "this-is-a-test-only-jwt-secret-key-with-enough-length-for-hmac-256";
        private const string Issuer   = "InclusiONTests";
        private const string Audience = "InclusiONTests";

        public static string ForProfessional(Guid userId) =>
            Build(userId, nameof(IdentityRoles.Professional), false, null,
                Permissions.Persons.Read,
                Permissions.Persons.Update,
                Permissions.Diagnoses.Read,
                Permissions.Diagnoses.Create,
                Permissions.Diagnoses.Update,
                Permissions.Reports.Read,
                Permissions.Reports.Create,
                Permissions.Reports.Submit);

        public static string ForFamilyRepresentative(Guid userId) =>
            Build(userId, nameof(IdentityRoles.FamilyRepresentative), false, null,
                Permissions.Persons.Read,
                Permissions.Reports.Read);

        public static string ForGlobalAdmin(Guid userId) =>
            Build(userId, nameof(IdentityRoles.Admin), true, null,
                Permissions.Persons.Read,
                Permissions.Persons.Update,
                Permissions.Diagnoses.Read,
                Permissions.Diagnoses.Create,
                Permissions.Reports.Read);

        private static string Build(
            Guid userId,
            string role,
            bool isGlobalAdmin,
            int[]? institutionIds,
            params string[] permissions)
        {
            // Usamos JsonWebTokenHandler (el mismo handler que usa el middleware JWT bearer
            // en .NET 8+) para garantizar compatibilidad de firma en Microsoft.IdentityModel 8.x.
            // JwtSecurityTokenHandler y JsonWebTokenHandler pueden producir firmas incompatibles
            // entre sí con las mismas claves en 8.0.x.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));

            // JsonWebTokenHandler NO aplica InboundClaimTypeMap, por lo que los claim types
            // se leen tal como se escriben en el JWT. Usamos los URIs completos de ClaimTypes
            // para que el servidor los encuentre con FindFirst(ClaimTypes.XXX) sin mapping.
            var claimsDict = new Dictionary<string, object>
            {
                [ClaimTypes.NameIdentifier]         = userId.ToString(),
                [ClaimTypes.Name]                   = "Test User",
                [ClaimTypes.Email]                  = "test@test.com",
                [ClaimTypes.Role]                   = role,
                [Permissions.IsActiveClaimType]     = "true",
                ["jti"]                             = Guid.NewGuid().ToString(),
            };

            // JsonWebTokenHandler serializa un string[] como JSON array → múltiples claims
            // del mismo tipo cuando el token se valida. Esto garantiza que c.Value == perm
            // funcione correctamente para cada permiso individual.
            if (permissions.Length == 1)
                claimsDict[Permissions.ClaimType] = permissions[0];
            else if (permissions.Length > 1)
                claimsDict[Permissions.ClaimType] = permissions;

            if (role == nameof(IdentityRoles.Admin))
            {
                claimsDict[Permissions.GlobalAdminClaimType] = isGlobalAdmin ? "true" : "false";

                if (!isGlobalAdmin && institutionIds is not null)
                    claimsDict[Permissions.InstitutionIdClaimType] =
                        institutionIds.Select(id => id.ToString()).ToArray();
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Claims            = claimsDict,
                Issuer            = Issuer,
                Audience          = Audience,
                NotBefore         = DateTime.UtcNow,
                Expires           = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}
