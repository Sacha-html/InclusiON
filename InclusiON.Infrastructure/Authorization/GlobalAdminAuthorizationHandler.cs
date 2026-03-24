using Microsoft.AspNetCore.Authorization;
using InclusiON.Application.Constants;

namespace InclusiON.Infrastructure.Authorization
{
    public class GlobalAdminRequirement : IAuthorizationRequirement { }

    public class GlobalAdminAuthorizationHandler : AuthorizationHandler<GlobalAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            GlobalAdminRequirement requirement)
        {
            if (context.User == null || !context.User.Identity?.IsAuthenticated == true)
            {
                return Task.CompletedTask;
            }

            var isGlobalAdmin = context.User.Claims
                .FirstOrDefault(c => c.Type == Permissions.GlobalAdminClaimType)?.Value == "true";

            if (isGlobalAdmin)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
