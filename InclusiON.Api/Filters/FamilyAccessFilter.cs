using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FamilyAccessAttribute : Attribute, IAsyncActionFilter
    {
        private readonly AccessMode _mode;
        private readonly string _routeParam;

        public FamilyAccessAttribute(AccessMode mode = AccessMode.Read, string routeParam = "familyId")
        {
            _mode = mode;
            _routeParam = routeParam;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_routeParam, out var raw) || raw is not Guid familyId)
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var resourceAuthz = services.GetRequiredService<IResourceAuthorizationService>();
            var ct = context.HttpContext.RequestAborted;

            if (await resourceAuthz.CanAccessFamilyAsync(familyId, _mode, ct))
            {
                await next();
                return;
            }

            var httpCtx = services.GetRequiredService<IHttpContextService>();
            var role = httpCtx.GetCurrentUserRole();

            context.Result = role switch
            {
                nameof(IdentityRoles.FamilyRepresentative) or nameof(IdentityRoles.PersonWithDisability)
                    => new NotFoundObjectResult(ApiResponse<object>.NotFound("Familiar")),
                _ => new ObjectResult(ApiResponse<object>.Forbidden()) { StatusCode = StatusCodes.Status403Forbidden }
            };
        }
    }
}
