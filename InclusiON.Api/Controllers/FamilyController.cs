using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Family;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class FamilyController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public FamilyController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        #region Queries

        [HttpGet]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<FamilyListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<FamilyListItemResponse>>>> GetFamily(
            [FromQuery] GetFamilyRequest request,
            [FromServices] IQueryHandler<GetFamilyQuery, ApiResponse<PagedResponse<FamilyListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var query = new GetFamilyQuery(
                request.Page, request.PageSize, request.Search, request.IsActive,
                request.SortBy, request.SortDirection, request.InstitutionIds, request.LinkedPersonSearch);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{familyId}")]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> GetFamilyById(
            Guid familyId,
            [FromServices] IQueryHandler<GetFamilyByIdQuery, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFamilyByIdQuery(familyId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("available")]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<FamilyResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<FamilyResponse>>>> GetAvailableFamilies(
            [FromServices] IQueryHandler<GetAvailableFamiliesQuery, ApiResponse<PagedResponse<FamilyResponse>>> handler,
            [FromQuery] string? search,
            [FromQuery] Guid? personId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAvailableFamiliesQuery(search, personId, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{familyId}/status-history")]
        [OutputCache(PolicyName = "history")]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<List<FamilyStatusHistoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FamilyStatusHistoryResponse>>>> GetFamilyStatusHistory(
            Guid familyId,
            [FromServices]
            IQueryHandler<GetFamilyStatusHistoryQuery, ApiResponse<List<FamilyStatusHistoryResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFamilyStatusHistoryQuery(familyId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{familyId}/link-history")]
        [OutputCache(PolicyName = "history")]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<List<PersonRepresentativeHistoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonRepresentativeHistoryResponse>>>> GetFamilyLinkHistory(
            Guid familyId,
            [FromServices]
            IQueryHandler<GetFamilyLinkHistoryQuery, ApiResponse<List<PersonRepresentativeHistoryResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFamilyLinkHistoryQuery(familyId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        #endregion

        #region Commands

        [HttpPost]
        [Authorize(Policy = "family:create")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> CreateFamily(
            [FromBody] CreateFamilyRequest request,
            [FromServices] ICommandHandler<CreateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateFamilyCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.DocumentNumber,
                request.Phone,
                request.Relationship,
                request.PersonId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetFamilyById),
                new { familyId = result.Data!.Id },
                result);
        }

        [HttpPut("{familyId}")]
        [Authorize(Policy = "family:update")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> UpdateFamily(
            Guid familyId,
            [FromBody] UpdateFamilyRequest request,
            [FromServices] ICommandHandler<UpdateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateFamilyCommand(
                familyId,
                request.FirstName,
                request.LastName,
                request.Email,
                request.DocumentNumber,
                request.Phone,
                request.Relationship);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{familyId}/deactivate")]
        [Authorize(Policy = "family:delete")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> DeactivateFamily(
            Guid familyId,
            [FromServices] ICommandHandler<DeactivateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivateFamilyCommand(familyId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{familyId}/link/{personId}")]
        [Authorize(Policy = "family:link")]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<PersonRepresentativeResponse>>> LinkFamilyToPerson(
            Guid familyId,
            Guid personId,
            [FromBody] LinkFamilyToPersonRequest request,
            [FromServices]
            ICommandHandler<LinkFamilyToPersonCommand, ApiResponse<PersonRepresentativeResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var command = new LinkFamilyToPersonCommand(
                familyId,
                personId,
                request.Relationship,
                request.IsPrimary,
                userId.Value);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetFamilyById),
                new { familyId },
                result);
        }

        [HttpDelete("{familyId}/unlink/{personId}")]
        [Authorize(Policy = "family:unlink")]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonRepresentativeResponse>>> UnlinkFamilyFromPerson(
            Guid familyId,
            Guid personId,
            [FromBody] UnlinkFamilyFromPersonRequest request,
            [FromServices]
            ICommandHandler<UnlinkFamilyFromPersonCommand, ApiResponse<PersonRepresentativeResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var command = new UnlinkFamilyFromPersonCommand(
                familyId,
                personId,
                request.Observation ?? string.Empty,
                userId.Value);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Professional Endpoints

        [HttpGet("professional/available")]
        [Authorize(Policy = "family:link")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<FamilyResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<FamilyResponse>>>> GetAvailableFamiliesForProfessional(
            [FromServices] IQueryHandler<GetAvailableFamiliesQuery, ApiResponse<PagedResponse<FamilyResponse>>> handler,
            [FromQuery] string? search,
            [FromQuery] Guid? personId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAvailableFamiliesQuery(search, personId, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("professional/link/{familyId}/{personId}")]
        [Authorize(Policy = "family:link")]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<PersonRepresentativeResponse>>> LinkFamilyToPersonAsProfessional(
            Guid familyId,
            Guid personId,
            [FromBody] LinkFamilyToPersonRequest request,
            [FromServices]
            ICommandHandler<LinkFamilyToPersonCommand, ApiResponse<PersonRepresentativeResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var command = new LinkFamilyToPersonCommand(
                familyId,
                personId,
                request.Relationship,
                request.IsPrimary,
                userId.Value);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetFamilyById),
                new { familyId },
                result);
        }

        [HttpDelete("professional/unlink/{familyId}/{personId}")]
        [Authorize(Policy = "family:unlink")]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonRepresentativeResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonRepresentativeResponse>>> UnlinkFamilyFromPersonAsProfessional(
            Guid familyId,
            Guid personId,
            [FromBody] UnlinkFamilyFromPersonRequest request,
            [FromServices]
            ICommandHandler<UnlinkFamilyFromPersonCommand, ApiResponse<PersonRepresentativeResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var command = new UnlinkFamilyFromPersonCommand(
                familyId,
                personId,
                request.Observation ?? string.Empty,
                userId.Value);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        // ────────────────────────────────────────────────────────────────
        // Dashboard Familiar
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Dashboard del familiar autenticado: personas vinculadas, actividades recientes,
        /// reportes aprobados y mensajes no leídos.
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FamilyDashboardResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<FamilyDashboardResponse>>> GetDashboard(
            [FromServices] IQueryHandler<GetFamilyDashboardQuery, ApiResponse<FamilyDashboardResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new GetFamilyDashboardQuery(userId.Value), cancellationToken);
            return Ok(result);
        }
    }
}