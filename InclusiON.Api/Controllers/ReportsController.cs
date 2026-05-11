using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Api.ModelBinders;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Enums;
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
        private readonly IResourceAuthorizationService _resourceAuthz;

        public ReportsController(
            IHttpContextService httpContextService,
            IResourceAuthorizationService resourceAuthz)
        {
            _httpContextService = httpContextService;
            _resourceAuthz = resourceAuthz;
        }

        // Usado para checks donde el personId viene del body o query string, no de la ruta.
        private ApiResponse<T> BuildDeniedResponse<T>(string resource = "Reporte") where T : class
        {
            var role = _httpContextService.GetCurrentUserRole();
            return role switch
            {
                nameof(IdentityRoles.FamilyRepresentative) or nameof(IdentityRoles.PersonWithDisability)
                    => ApiResponse<T>.NotFound(resource),
                _ => ApiResponse<T>.Forbidden()
            };
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

            // Si el caller filtra por persona, verificar que tiene acceso a esa persona.
            if (Guid.TryParse(request.PersonId, out var filteredPersonId)
                && !await _resourceAuthz.CanAccessPersonAsync(filteredPersonId, AccessMode.Read, cancellationToken))
            {
                return BuildDeniedResponse<PagedResponse<ReportsListItemReponse>>("Persona").ToActionResult();
            }

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
            var familyId = _httpContextService.GetCurrentEntityId();
            if (familyId is null)
                return BadRequest(ApiResponse<PagedResponse<ReportsListItemReponse>>.ErrorResult("Solo los familiares pueden acceder a este endpoint."));

            request.Validate();

            var query = new GetFamilyReportsQuery(
                familyId.Value,
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
        [HttpGet("{reportId}")]
        [Authorize(Policy = "reports:read")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        [ReportAccess(AccessMode.Read)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> GetReportById(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
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

            if (!await _resourceAuthz.CanAccessPersonAsync(request.PersonId, AccessMode.Write, cancellationToken))
            {
                return BuildDeniedResponse<ReportResponse>().ToActionResult();
            }

            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden crear reportes."));

            var command = new CreateReportCommand(
                request.PersonId,
                professionalId.Value,
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
        [HttpPut("{reportId}")]
        [Authorize(Policy = "reports:create")]
        [ReportAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> UpdateReport(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
            [FromBody] UpdateReportRequest request,
            [FromServices] ICommandHandler<UpdateReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Datos inválidos"));

            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden editar reportes."));

            var command = new UpdateReportCommand(
                reportId,
                professionalId.Value,
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
        [HttpPatch("{reportId}/submit")]
        [Authorize(Policy = "reports:submit")]
        [ReportAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> SubmitReport(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
            [FromServices] ICommandHandler<SubmitReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<ReportResponse>.ErrorResult("Solo los profesionales pueden enviar reportes."));

            var result = await handler.HandleAsync(new SubmitReportCommand(reportId, professionalId.Value), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Admin aprueba el reporte. El familiar podrá consultarlo.</summary>
        [HttpPatch("{reportId}/approve")]
        [Authorize(Policy = "reports:approve")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> ApproveReport(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
            [FromServices] ICommandHandler<ApproveReportCommand, ApiResponse<ReportResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var adminUserId = _httpContextService.GetCurrentUserId()!.Value;
            var result = await handler.HandleAsync(new ApproveReportCommand(reportId, adminUserId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Profesional da de baja su reporte (baja lógica). No permitido en estado Enviado.</summary>
        [HttpPut("{reportId}/deactivate")]
        [Authorize(Policy = "reports:create")]
        [ReportAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateReport(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
            [FromServices] ICommandHandler<DeactivateReportCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<object>.ErrorResult("Solo los profesionales pueden dar de baja reportes."));

            var result = await handler.HandleAsync(new DeactivateReportCommand(reportId, professionalId.Value), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>Admin rechaza el reporte con un motivo para el profesional.</summary>
        [HttpPatch("{reportId}/reject")]
        [Authorize(Policy = "reports:reject")]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ReportResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> RejectReport(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int reportId,
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
