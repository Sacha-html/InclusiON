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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IFamilyRepository _familyRepository;

        public ReportsController(
            IHttpContextService httpContextService,
            IProfessionalsRepository professionalsRepository,
            IFamilyRepository familyRepository)
        {
            _httpContextService = httpContextService;
            _professionalsRepository = professionalsRepository;
            _familyRepository = familyRepository;
        }

        /// <summary>Lista paginada de reportes con filtros.</summary>
        [HttpGet]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status200OK)]
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
                request.Status,
                request.DateFrom,
                request.DateTo,
                request.SortBy,
                request.SortDirection,
                request.InstitutionIds
            );

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>Reportes aprobados del familiar autenticado (solo sus personas a cargo).</summary>
        [HttpGet("family")]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ReportsListItemReponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ReportsListItemReponse>>>> GetFamilyReports(
            [FromQuery] GetReportsRequest request,
            [FromServices] IQueryHandler<GetFamilyReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var family = await _familyRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (family is null)
                return BadRequest(ApiResponse<PagedResponse<ReportsListItemReponse>>.ErrorResult("Solo los familiares pueden acceder a este endpoint."));

            request.Validate();

            var query = new GetFamilyReportsQuery(
                family.Id,
                request.Page,
                request.PageSize,
                request.ReportTypeId,
                request.DateFrom,
                request.DateTo,
                request.SortBy,
                request.SortDirection
            );

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>Obtiene un reporte por ID.</summary>
        [HttpGet("{reportId:int}")]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> GetReportById(
            int reportId,
            [FromServices] IQueryHandler<GetReportByIdQuery, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetReportByIdQuery(reportId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Crea un nuevo reporte (nace como Borrador).</summary>
        [HttpPost]
        [Authorize(Policy = "reports:create")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> CreateReport(
            [FromBody] CreateReportRequest request,
            [FromServices] ICommandHandler<CreateReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Datos inválidos"));

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

        /// <summary>Edita un reporte. Solo permitido cuando Status == Draft.</summary>
        [HttpPut("{reportId:int}")]
        [Authorize(Policy = "reports:create")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> UpdateReport(
            int reportId,
            [FromBody] UpdateReportRequest request,
            [FromServices] ICommandHandler<UpdateReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Datos inválidos"));

            var currentUserId = _httpContextService.GetCurrentUserId()!.Value;
            var professional = await _professionalsRepository.GetByUserIdAsync(currentUserId, cancellationToken);

            if (professional is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden editar reportes."));

            var command = new UpdateReportCommand(
                reportId,
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

        /// <summary>Profesional envía el borrador al admin para revisión.</summary>
        [HttpPatch("{reportId:int}/submit")]
        [Authorize(Policy = "reports:submit")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> SubmitReport(
            int reportId,
            [FromServices] ICommandHandler<SubmitReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _httpContextService.GetCurrentUserId()!.Value;
            var professional = await _professionalsRepository.GetByUserIdAsync(currentUserId, cancellationToken);

            if (professional is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden enviar reportes."));

            var result = await handler.HandleAsync(new SubmitReportCommand(reportId, professional.Id), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Admin aprueba el reporte. El familiar podrá consultarlo.</summary>
        [HttpPatch("{reportId:int}/approve")]
        [Authorize(Policy = "reports:approve")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> ApproveReport(
            int reportId,
            [FromServices] ICommandHandler<ApproveReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var adminUserId = _httpContextService.GetCurrentUserId()!.Value;
            var result = await handler.HandleAsync(new ApproveReportCommand(reportId, adminUserId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Admin rechaza el reporte con un motivo para el profesional.</summary>
        [HttpPatch("{reportId:int}/reject")]
        [Authorize(Policy = "reports:reject")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> RejectReport(
            int reportId,
            [FromBody] RejectReportRequest request,
            [FromServices] ICommandHandler<RejectReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("El motivo del rechazo es obligatorio."));

            var adminUserId = _httpContextService.GetCurrentUserId()!.Value;
            var result = await handler.HandleAsync(new RejectReportCommand(reportId, adminUserId, request.Comment), cancellationToken);
            return result.ToActionResult();
        }
    }
}
