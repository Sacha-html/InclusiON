
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
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
        private readonly IProfessionalsRepository _professionalsRepository;

        public ReportsController(
            IHttpContextService httpContextService,
            IProfessionalsRepository professionalsRepository)
        {
            _httpContextService = httpContextService;
            _professionalsRepository = professionalsRepository;
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

        /// <summary>
        /// Obtiene un reporte por su ID.
        /// </summary>
        [HttpGet("{reportId:int}")]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> GetReportById(
            int reportId,
            [FromServices] IQueryHandler<GetReportByIdQuery, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetReportByIdQuery(reportId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Crea un nuevo reporte.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "reports:create")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> CreateReport(
            [FromBody] CreateReportRequest request,
            [FromServices] ICommandHandler<CreateReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Datos inválidos"));
            }

            var currentUserId = _httpContextService.GetCurrentUserId()!.Value;
            var professional = await _professionalsRepository.GetByUserIdAsync(currentUserId, cancellationToken);

            if (professional is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden crear reportes."));

            var command = new CreateReportCommand(
                request.PersonId,
                professional.Id,
                request.Title,
                request.Content,
                request.ReportTypeId,
                request.ReportDate,
                request.PeriodStartDate,
                request.PeriodEndDate,
                request.AchievedGoals,
                request.AreasToReinforce,
                request.FutureRecommendations,
                request.NextObjectives
            );

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }
    }
}
