using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace InclusiON.Infrastructure.Authorization
{
    /// <summary>
    /// Provider dinámico de políticas de autorización.
    /// Permite usar [Authorize(Policy = "users:read")] sin registrar cada política manualmente.
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private const string PermissionPrefix = ""; // Sin prefijo, el nombre del permiso es la política

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Si el nombre de la política contiene ":", es un permiso
            if (policyName.Contains(':'))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(policyName))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Si no, usar el provider por defecto (políticas registradas manualmente)
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
