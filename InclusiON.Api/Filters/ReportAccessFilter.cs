using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Verifica que el usuario autenticado tenga acceso al reporte indicado en la ruta (HU-IN-172).
    /// Resuelve PersonId desde el reporte y delega al check canónico de persona.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReportAccessAttribute : Attribute, IAsyncActionFilter
    {
        private readonly AccessMode _mode;
        private readonly string _routeParam;

        public ReportAccessAttribute(AccessMode mode = AccessMode.Read, string routeParam = "reportId")
        {
            _mode = mode;
            _routeParam = routeParam;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_routeParam, out var raw) || raw is not int reportId)
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var resourceAuthz = services.GetRequiredService<IResourceAuthorizationService>();
            var ct = context.HttpContext.RequestAborted;

            if (await resourceAuthz.CanAccessReportAsync(reportId, _mode, ct))
            {
                await next();
                return;
            }

            var httpCtx = services.GetRequiredService<IHttpContextService>();
            var role = httpCtx.GetCurrentUserRole();

            context.Result = role switch
            {
                nameof(IdentityRoles.FamilyRepresentative) or nameof(IdentityRoles.PersonWithDisability)
                    => new NotFoundObjectResult(ApiResponse<object>.NotFound("Reporte")),
                _ => new ObjectResult(ApiResponse<object>.Forbidden()) { StatusCode = StatusCodes.Status403Forbidden }
            };
        }
    }
}
