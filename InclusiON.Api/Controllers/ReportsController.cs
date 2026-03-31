
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Reports;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using InclusiON.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        public ReportsController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        /// <summary>
        /// Obtiene una lista paginada de reportes.
        /// </summary>        
        [HttpGet]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ReportsListItemReponse>>>> GetReports(
            [FromQuery] GetReportsRequest request,
            [FromServices] IQueryHandler<GetReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var query = new GetReportsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.PersonId,
                request.ProfessionalId,
                request.ReportTypeId,
                request.IsActive,
                request.SortBy,
                request.SortDirection,
                request.InstitutionIds
            );

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result); 
        }
    }
}
