using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Professionals;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
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

            var query = new GetProfessionalsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.Specialty,
                request.IsActive,
                request.SortBy,
                request.SortDirection,
                request.InstitutionId);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un profesional por su ID.
        /// </summary>
        [HttpGet("{professionalId:guid}")]
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
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<ProfessionalResponse>.Unauthorized());
            }

            // Usamos un query especial: buscamos por UserId, no por ProfessionalId
            // Para reutilizar el handler, necesitamos buscar primero el professional
            // Inyectamos el repository directamente para /me
            return await GetProfessionalByUserId(userId.Value, handler, cancellationToken);
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
                request.DocumentNumber,
                request.Phone,
                request.Specialty,
                request.LicenseNumber,
                request.BirthDate,
                request.Address,
                request.Email);

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
        [HttpPut("{professionalId:guid}")]
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

            var command = new UpdateProfessionalCommand(
                professionalId,
                request.FirstName,
                request.LastName,
                request.DocumentNumber,
                request.Phone,
                request.Specialty,
                request.LicenseNumber,
                request.BirthDate,
                request.Address);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva un profesional (soft delete).
        /// </summary>
        [HttpPut("{professionalId:guid}/deactivate")]
        [Authorize(Policy = "professionals:delete")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProfessionalResponse>>> DeactivateProfessional(
            Guid professionalId,
            [FromServices] ICommandHandler<DeactivateProfessionalCommand, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivateProfessionalCommand(professionalId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Private Methods

        private async Task<ActionResult<ApiResponse<ProfessionalResponse>>> GetProfessionalByUserId(
            Guid userId,
            IQueryHandler<GetProfessionalByIdQuery, ApiResponse<ProfessionalResponse>> handler,
            CancellationToken cancellationToken)
        {
            // Para /me necesitamos buscar por UserId, pero el handler busca por ProfessionalId.
            // Inyectamos el repositorio para esta operacion especifica.
            var repository = HttpContext.RequestServices.GetRequiredService<InclusiON.Application.Interfaces.Repositories.IProfessionalsRepository>();
            var professional = await repository.GetByUserIdAsync(userId, cancellationToken);

            if (professional == null)
            {
                return NotFound(ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound));
            }

            var query = new GetProfessionalByIdQuery(professional.Id);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion
    }
}
