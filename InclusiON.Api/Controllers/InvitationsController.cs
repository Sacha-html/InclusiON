using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using InclusiON.Api.Extensions;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Invitations;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class InvitationsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IResourceAuthorizationService _resourceAuthz;
        private readonly string[] _allowedOrigins;

        public InvitationsController(
            IHttpContextService httpContextService,
            IResourceAuthorizationService resourceAuthz,
            IConfiguration configuration)
        {
            _httpContextService  = httpContextService;
            _resourceAuthz       = resourceAuthz;
            _allowedOrigins      = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        }

        #region Queries

        /// <summary>
        /// Obtiene la lista de invitaciones. Profesional: solo las suyas. Admin: todas.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "invitations:read")]
        [ProducesResponseType(typeof(ApiResponse<List<InvitationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<InvitationResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<List<InvitationResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<InvitationResponse>>>> GetInvitations(
            [FromServices] IQueryHandler<GetInvitationsQuery, ApiResponse<List<InvitationResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            // Profesional: filtra por sus propias invitaciones (entityId = professionalId en el JWT).
            // Admin institucional: filtra por sus instituciones (institutionIds en el JWT).
            // GlobalAdmin: sin filtros.
            var professionalId = _httpContextService.GetCurrentEntityId();

            GetInvitationsQuery query;
            if (professionalId != null)
            {
                // Professional: solo sus invitaciones
                query = new GetInvitationsQuery(professionalId);
            }
            else
            {
                var institutionIds = _httpContextService.GetInstitutionIds();
                query = institutionIds.Count > 0
                    ? new GetInvitationsQuery(null, institutionIds)   // Admin institucional
                    : new GetInvitationsQuery(null);                  // GlobalAdmin
            }

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Valida un codigo de invitacion y devuelve los datos pre-llenados.
        /// </summary>
        [HttpGet("{code}")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        [ProducesResponseType(typeof(ApiResponse<InvitationValidationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<InvitationValidationResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<InvitationValidationResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<InvitationValidationResponse>>> ValidateInvitation(
            string code,
            [FromServices] IQueryHandler<ValidateInvitationQuery, ApiResponse<InvitationValidationResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new ValidateInvitationQuery(code);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Crea una nueva invitacion para un representante familiar.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "invitations:create")]
        [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<InvitationResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<InvitationResponse>>> CreateInvitation(
            [FromBody] CreateInvitationRequest request,
            [FromServices] ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>> handler,
            CancellationToken cancellationToken = default)
        {

            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId == null)
            {
                return NotFound(ApiResponse<InvitationResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound));
            }

            // Si la invitacion apunta a una persona, verificar que el profesional la tiene asignada.
            if (request.PersonId.HasValue
                && !await _resourceAuthz.CanAccessPersonAsync(request.PersonId.Value, AccessMode.Write, cancellationToken))
            {
                return Forbid();
            }

            // Armar la URL base del cliente para el link de invitación.
            // Se valida contra la whitelist de CORS para evitar phishing por header injection:
            // un atacante no puede redirigir el link a un dominio arbitrario manipulando Origin/Referer.
            var requestOrigin = Request.Headers["Origin"].FirstOrDefault();
            var baseUrl = _allowedOrigins.Contains(requestOrigin, StringComparer.OrdinalIgnoreCase)
                ? requestOrigin
                : _allowedOrigins.FirstOrDefault();

            var command = new CreateInvitationCommand(
                professionalId.Value,
                request.PersonId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Relationship,
                baseUrl);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(ValidateInvitation),
                new { code = result.Data!.Code },
                result);
        }

        /// <summary>
        /// Acepta una invitacion y registra al representante familiar.
        /// </summary>
        [HttpPost("{code}/accept")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-sensitive")]
        [ProducesResponseType(typeof(ApiResponse<AcceptInvitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcceptInvitationResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AcceptInvitationResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AcceptInvitationResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AcceptInvitationResponse>>> AcceptInvitation(
            string code,
            [FromBody] AcceptInvitationRequest request,
            [FromServices] ICommandHandler<AcceptInvitationCommand, ApiResponse<AcceptInvitationResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new AcceptInvitationCommand(
                code,
                request.Email,
                request.Password,
                request.ConfirmPassword);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

    }
}
