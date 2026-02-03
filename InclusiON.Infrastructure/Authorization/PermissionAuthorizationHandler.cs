using Microsoft.AspNetCore.Authorization;
using InclusiON.ApplicationBusiness.Constants;

namespace InclusiON.Infrastructure.Authorization
{
    /// <summary>
    /// Requirement que especifica el permiso requerido.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// Handler que verifica si el usuario tiene el permiso requerido.
    /// Los permisos se obtienen de los claims del rol (AspNetRoleClaims).
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
            {
                return Task.CompletedTask;
            }

            // Verificar si el usuario tiene el permiso como claim
            var hasPermission = context.User.Claims
                .Any(c => c.Type == Permissions.ClaimType && c.Value == requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
