using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Verifica que el usuario autenticado tenga vinculo activo con la persona indicada en la ruta (HU-IN-172).
    /// Devuelve 404 para FamilyRepresentative/PersonWithDisability (oculta existencia del recurso)
    /// y 403 para Professional/Admin (feedback claro). El audit queda registrado por IAccessAuditLogger.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PersonAccessAttribute : Attribute, IAsyncActionFilter
    {
        private readonly AccessMode _mode;
        private readonly string _routeParam;

        public PersonAccessAttribute(AccessMode mode = AccessMode.Read, string routeParam = "personId")
        {
            _mode = mode;
            _routeParam = routeParam;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(_routeParam, out var raw) || raw is not Guid personId)
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var resourceAuthz = services.GetRequiredService<IResourceAuthorizationService>();
            var ct = context.HttpContext.RequestAborted;

            if (await resourceAuthz.CanAccessPersonAsync(personId, _mode, ct))
            {
                await next();
                return;
            }

            var httpCtx = services.GetRequiredService<IHttpContextService>();
            var role = httpCtx.GetCurrentUserRole();

            context.Result = role switch
            {
                nameof(IdentityRoles.FamilyRepresentative) or nameof(IdentityRoles.PersonWithDisability)
                    => new NotFoundObjectResult(ApiResponse<object>.NotFound("Persona")),
                _ => new ObjectResult(ApiResponse<object>.Forbidden()) { StatusCode = StatusCodes.Status403Forbidden }
            };
        }
    }
}
