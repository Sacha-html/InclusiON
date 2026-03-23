using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Application.UseCases.Invitations.Queries;
using InclusiON.Data;
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
        private readonly AppDbContext _context;

        public InvitationsController(
            IHttpContextService httpContextService,
            AppDbContext context)
        {
            _httpContextService = httpContextService;
            _context = context;
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
            // Si es profesional, filtrar por sus invitaciones.
            var professionalId = await GetCurrentProfessionalId(cancellationToken);

            GetInvitationsQuery query;
            if (professionalId != null)
            {
                // Professional: only their invitations
                query = new GetInvitationsQuery(professionalId);
            }
            else
            {
                // Admin: check if institutional
                var userId = _httpContextService.GetCurrentUserId();
                var adminInstitutions = userId.HasValue
                    ? await _context.AdminInstitutions
                        .Where(ai => ai.AdminUserId == userId.Value && ai.IsActive)
                        .Select(ai => ai.InstitutionId)
                        .ToListAsync(cancellationToken)
                    : new List<int>();

                if (adminInstitutions.Any())
                {
                    // Institutional admin: filter by their institutions
                    query = new GetInvitationsQuery(null, adminInstitutions);
                }
                else
                {
                    // Global admin: all invitations
                    query = new GetInvitationsQuery(null);
                }
            }

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Valida un codigo de invitacion y devuelve los datos pre-llenados.
        /// </summary>
        [HttpGet("{code}")]
        [AllowAnonymous]
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<InvitationResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var professionalId = await GetCurrentProfessionalId(cancellationToken);
            if (professionalId == null)
            {
                return NotFound(ApiResponse<InvitationResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound));
            }

            // Obtener la URL base del cliente para armar el link de invitacion
            var baseUrl = Request.Headers["Origin"].FirstOrDefault()
                       ?? Request.Headers["Referer"].FirstOrDefault()?.TrimEnd('/');

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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<AcceptInvitationResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new AcceptInvitationCommand(
                code,
                request.Email,
                request.Password,
                request.ConfirmPassword);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Private Methods

        private async Task<Guid?> GetCurrentProfessionalId(CancellationToken cancellationToken)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null) return null;

            var repository = HttpContext.RequestServices.GetRequiredService<IProfessionalsRepository>();
            var professional = await repository.GetByUserIdAsync(userId.Value, cancellationToken);
            return professional?.Id;
        }

        #endregion
    }
}
