using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InclusiON.Application.Constants;
using InclusiON.Domain.Enums;
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
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, "Test User"),
                new(ClaimTypes.Email, "test@test.com"),
                new(ClaimTypes.Role, role),
                new(Permissions.IsActiveClaimType, "true"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var perm in permissions)
                claims.Add(new Claim(Permissions.ClaimType, perm));

            if (role == nameof(IdentityRoles.Admin))
            {
                claims.Add(new Claim(Permissions.GlobalAdminClaimType, isGlobalAdmin ? "true" : "false"));

                if (!isGlobalAdmin && institutionIds is not null)
                    foreach (var id in institutionIds)
                        claims.Add(new Claim(Permissions.InstitutionIdClaimType, id.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
