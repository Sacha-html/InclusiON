using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Verifica que el usuario autenticado tenga acceso al diagnóstico indicado en la ruta (HU-IN-172).
    /// Resuelve PersonId desde el diagnóstico y delega al check canónico de persona.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DiagnosisAccessAttribute : Attribute, IAsyncActionFilter
    {
        private readonly AccessMode _mode;
        private readonly string _routeParam;

        public DiagnosisAccessAttribute(AccessMode mode = AccessMode.Read, string routeParam = "id")
        {
            _mode = mode;
            _routeParam = routeParam;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_routeParam, out var raw) || raw is not int diagnosisId)
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var resourceAuthz = services.GetRequiredService<IResourceAuthorizationService>();
            var ct = context.HttpContext.RequestAborted;

            if (await resourceAuthz.CanAccessDiagnosisAsync(diagnosisId, _mode, ct))
            {
                await next();
                return;
            }

            var httpCtx = services.GetRequiredService<IHttpContextService>();
            var role = httpCtx.GetCurrentUserRole();

            context.Result = role switch
            {
                nameof(IdentityRoles.FamilyRepresentative) or nameof(IdentityRoles.PersonWithDisability)
                    => new NotFoundObjectResult(ApiResponse<object>.NotFound("Diagnóstico")),
                _ => new ObjectResult(ApiResponse<object>.Forbidden()) { StatusCode = StatusCodes.Status403Forbidden }
            };
        }
    }
}
