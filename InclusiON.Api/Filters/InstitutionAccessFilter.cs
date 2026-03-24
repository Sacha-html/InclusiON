using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Filtro que aplica enforcement de acceso por institucion para admins no-globales.
    /// Para requests que implementen IInstitutionFilterable:
    /// - Global admin: pasa sin restriccion.
    /// - Admin institucional: valida y fuerza el filtro por sus instituciones asignadas.
    /// - Otros roles (Professional, Family): pasan sin cambios (tienen su propio scoping).
    /// </summary>
    public class InstitutionAccessFilter : IAsyncActionFilter
    {
        private readonly IHttpContextService _httpContextService;

        public InstitutionAccessFilter(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var filterable = context.ActionArguments.Values
                .OfType<IInstitutionFilterable>()
                .FirstOrDefault();

            if (filterable is null)
            {
                await next();
                return;
            }

            // Global admins pasan sin restriccion
            if (_httpContextService.IsGlobalAdmin())
            {
                if (filterable.InstitutionId.HasValue)
                {
                    filterable.InstitutionIds = new List<int> { filterable.InstitutionId.Value };
                }
                // Si no mando filtro, InstitutionIds queda null = ve todo

                await next();
                return;
            }

            var allowedIds = _httpContextService.GetInstitutionIds();

            // Si no tiene claims de institucion, no es admin institucional (ej: Professional, Family)
            if (allowedIds.Count == 0)
            {
                await next();
                return;
            }

            // Admin institucional: enforcement
            if (filterable.InstitutionId.HasValue)
            {
                if (!allowedIds.Contains(filterable.InstitutionId.Value))
                {
                    context.Result = new ObjectResult(
                        ApiResponse<object>.Forbidden("No tiene acceso a esta institución."))
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }

                filterable.InstitutionIds = new List<int> { filterable.InstitutionId.Value };
            }
            else
            {
                // No especifico institucion: auto-filtrar por todas sus instituciones
                filterable.InstitutionIds = allowedIds;
            }

            await next();
        }
    }
}
