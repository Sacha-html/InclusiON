using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Api.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Produces("application/json")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public AdminDashboardController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        /// <summary>
        /// Dashboard administrativo con KPIs.
        /// GlobalAdmin: estadísticas globales (incluye TotalInstitutions).
        /// AdminInstitucional: estadísticas acotadas a sus instituciones.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "users:read")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard(
            [FromServices] IQueryHandler<GetAdminDashboardQuery, ApiResponse<AdminDashboardResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var isGlobalAdmin  = _httpContextService.IsGlobalAdmin();
            var institutionIds = _httpContextService.GetInstitutionIds();

            var query  = new GetAdminDashboardQuery(isGlobalAdmin, institutionIds);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }
    }
}
