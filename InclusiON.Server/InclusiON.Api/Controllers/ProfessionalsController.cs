using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Professionals;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Enums;
using InclusiON.Shared.Resources;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestion de profesionales.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProfessionalsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public ProfessionalsController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        #region Queries

        /// <summary>
        /// Obtiene una lista paginada de profesionales.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "professionals:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProfessionalListItemResponse>>>> GetProfessionals(
            [FromQuery] GetProfessionalsRequest request,
            [FromServices] IQueryHandler<GetProfessionalsQuery, ApiResponse<PagedResponse<ProfessionalListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue)
                return ApiResponse<PagedResponse<ProfessionalListItemResponse>>.Forbidden().ToActionResult();

            var query = new GetProfessionalsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.Specialty,
                request.IsActive,
                request.Status,
                request.SortBy,
                request.SortDirection,
                request.InstitutionIds);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene profesionales pendientes de validacion.
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Policy = "professionals:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProfessionalListItemResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProfessionalListItemResponse>>>> GetPendingProfessionals(
            [FromQuery] GetPendingProfessionalsRequest request,
            [FromServices] IQueryHandler<GetPendingProfessionalsQuery, ApiResponse<PagedResponse<ProfessionalListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue)
                return ApiResponse<PagedResponse<ProfessionalListItemResponse>>.Forbidden().ToActionResult();

            var query = new GetPendingProfessionalsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortDirection);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un profesional por su ID.
        /// </summary>
        [HttpGet("{professionalId}")]
        [Authorize(Policy = "professionals:read")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> GetProfessionalById(
            Guid professionalId,
            [FromServices] IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<ProfessionalResponse>.Forbidden().ToActionResult();

            var query = new GetProfessionalByIdQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el perfil del profesional autenticado.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> GetMyProfile(
            [FromServices] IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            // El professionalId viene encriptado en el JWT — sin consulta adicional a BD.
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
            {
                return NotFound(ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound));
            }

            var query = new GetProfessionalByIdQuery(professionalId.Value);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Devuelve el resumen de progreso semanal del profesional autenticado (últimos 7 días).
        /// </summary>
        [HttpGet("me/weekly-progress")]
        [Authorize(Roles = "Professional")]
        [ProducesResponseType(typeof(ApiResponse<WeeklyProgressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<WeeklyProgressResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<WeeklyProgressResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<WeeklyProgressResponse>>> GetWeeklyProgress(
            [FromServices] IQueryHandler<GetWeeklyProgressQuery, ApiResponse<WeeklyProgressResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
            {
                return Unauthorized(ApiResponse<WeeklyProgressResponse>.ErrorResult(
                    ErrorCode.Unauthorized,
                    "No autenticado"));
            }

            var query  = new GetWeeklyProgressQuery(professionalId.Value);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Registro público de un profesional. Queda pendiente de validación.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> RegisterProfessional(
            [FromBody] RegisterProfessionalRequest request,
            [FromServices] ICommandHandler<RegisterProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new RegisterProfessionalCommand(
                request.FirstName,
                request.LastName,
                request.DocumentNumber,
                request.Phone,
                request.Specialty,
                request.LicenseNumber,
                request.BirthDate,
                request.Email,
                request.InstitutionId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetProfessionalById),
                new { professionalId = result.Data!.Id },
                result);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Crea un nuevo profesional.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "professionals:create")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> CreateProfessional(
            [FromBody] CreateProfessionalRequest request,
            [FromServices] ICommandHandler<CreateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {

            var command = new CreateProfessionalCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.DocumentNumber,
                request.Phone,
                request.Specialty,
                request.LicenseNumber,
                request.BirthDate,
                request.InstitutionIds);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetProfessionalById),
                new { professionalId = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Actualiza un profesional existente.
        /// </summary>
        [HttpPut("{professionalId}")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> UpdateProfessional(
            Guid professionalId,
            [FromBody] UpdateProfessionalRequest request,
            [FromServices] ICommandHandler<UpdateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<ProfessionalResponse>.Forbidden().ToActionResult();

            var command = new UpdateProfessionalCommand(
                professionalId,
                request.FirstName,
                request.LastName,
                request.DocumentNumber,
                request.Phone,
                request.Specialty,
                request.LicenseNumber,
                request.BirthDate,
                request.InstitutionIds);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva un profesional (soft delete).
        /// </summary>
        [HttpPut("{professionalId}/deactivate")]
        [Authorize(Policy = "professionals:delete")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> DeactivateProfessional(
            Guid professionalId,
            [FromBody] DeactivateProfessionalRequest? request,
            [FromServices] ICommandHandler<DeactivateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<ProfessionalResponse>.Forbidden().ToActionResult();

            var command = new DeactivateProfessionalCommand(professionalId, request?.Observation);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Valida (aprobar o rechazar) un profesional registrado.
        /// </summary>
        [HttpPut("{professionalId}/validate")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> ValidateProfessional(
            Guid professionalId,
            [FromBody] ValidateProfessionalRequest request,
            [FromServices] ICommandHandler<ValidateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<ProfessionalResponse>.Forbidden().ToActionResult();

            var command = new ValidateProfessionalCommand(
                professionalId,
                request.IsApproved,
                request.Observation);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Reactiva un profesional dado de baja o suspendido.
        /// </summary>
        [HttpPut("{professionalId}/reactivate")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> ReactivateProfessional(
            Guid professionalId,
            [FromBody] ReactivateProfessionalRequest? request,
            [FromServices] ICommandHandler<ReactivateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<ProfessionalResponse>.Forbidden().ToActionResult();

            var command = new ReactivateProfessionalCommand(professionalId, request?.Observation);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el historial de estados de un profesional.
        /// </summary>
        [HttpGet("{professionalId}/status-history")]
        [OutputCache(PolicyName = "history")]
        [Authorize(Policy = "professionals:read")]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalStatusHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalStatusHistoryResponse>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalStatusHistoryResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<ProfessionalStatusHistoryResponse>>>> GetStatusHistory(
            Guid professionalId,
            [FromServices] IQueryHandler<GetProfessionalStatusHistoryQuery, ApiResponse<List<ProfessionalStatusHistoryResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<List<ProfessionalStatusHistoryResponse>>.Forbidden().ToActionResult();

            var query = new GetProfessionalStatusHistoryQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Suspende profesionales que no han iniciado sesión en los últimos días.
        /// </summary>
        [HttpPost("suspend-inactive")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<SuspendResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<SuspendResult>>> SuspendInactiveProfessionals(
            [FromQuery] int days,
            [FromServices] ICommandHandler<SuspendInactiveProfessionalsCommand, ApiResponse<SuspendResult>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new SuspendInactiveProfessionalsCommand(days > 0 ? days : 90);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

    }
}
