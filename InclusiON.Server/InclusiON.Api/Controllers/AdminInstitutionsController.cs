using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Api.Extensions;
using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.Application.Constants;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Admin;
using InclusiON.DTOs.Requests.Assignments;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Api.Controllers
{
    [Route("api/admin/institutions-assignments")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = Permissions.Settings.Update)]
    public class AdminInstitutionsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IOutputCacheStore _cacheStore;
        private readonly IEncryptionService _encryption;

        public AdminInstitutionsController(IHttpContextService httpContextService, IOutputCacheStore cacheStore, IEncryptionService encryption)
        {
            _httpContextService = httpContextService;
            _cacheStore         = cacheStore;
            _encryption         = encryption;
        }

        [HttpGet("admins")]
        [OutputCache(PolicyName = "admins")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminUserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<AdminUserResponse>>>> GetAllAdmins(
            [FromServices] IQueryHandler<GetAllAdminsQuery, ApiResponse<PagedResponse<AdminUserResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetAllAdminsQuery(page, pageSize, search), cancellationToken);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminInstitutionResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PagedResponse<AdminInstitutionResponse>>>> GetMyInstitutions(
            [FromServices] IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<PagedResponse<AdminInstitutionResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null) return Unauthorized(ApiResponse<PagedResponse<AdminInstitutionResponse>>.Unauthorized());

            var result = await handler.HandleAsync(new GetAdminInstitutionsQuery(userId.Value, page, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{adminUserId}")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminInstitutionResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResponse<AdminInstitutionResponse>>>> GetAdminInstitutions(
            Guid adminUserId,
            [FromServices] IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<PagedResponse<AdminInstitutionResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetAdminInstitutionsQuery(adminUserId, page, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpPost("{adminUserId}")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> AssignInstitution(
            Guid adminUserId,
            [FromBody] AssignInstitutionToAdminRequest request,
            [FromServices] ICommandHandler<AssignInstitutionToAdminCommand, ApiResponse<AdminInstitutionResponse>> handler,
            CancellationToken cancellationToken)
        {
            if (!_encryption.TryDecryptId(request.InstitutionId, out var institutionId))
                return BadRequest(ApiResponse<AdminInstitutionResponse>.ErrorResult("ID de institución inválido."));

            var command = new AssignInstitutionToAdminCommand(adminUserId, institutionId);
            var result  = await handler.HandleAsync(command, cancellationToken);

            if (result.Success)
                await _cacheStore.EvictByTagAsync("admins", cancellationToken);

            return result.ToActionResult();
        }

        [HttpDelete("{adminUserId}/{encryptedInstitutionId}")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> RemoveAssignment(
            Guid adminUserId,
            string encryptedInstitutionId,
            [FromServices] ICommandHandler<RemoveAdminInstitutionCommand, ApiResponse<AdminInstitutionResponse>> handler,
            CancellationToken cancellationToken)
        {
            if (!_encryption.TryDecryptId(encryptedInstitutionId, out var institutionId))
                return BadRequest(ApiResponse<AdminInstitutionResponse>.ErrorResult("ID de institución inválido."));

            var command = new RemoveAdminInstitutionCommand(adminUserId, institutionId);
            var result  = await handler.HandleAsync(command, cancellationToken);

            if (result.Success)
                await _cacheStore.EvictByTagAsync("admins", cancellationToken);

            return result.ToActionResult();
        }

        [HttpPost("users")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CreateAdminUserResponse>>> CreateAdminUser(
            [FromBody] CreateAdminUserRequest request,
            [FromServices] ICommandHandler<CreateAdminUserCommand, ApiResponse<CreateAdminUserResponse>> handler,
            CancellationToken cancellationToken)
        {
            var command = new CreateAdminUserCommand(request.Email, request.FirstName, request.LastName, request.InstitutionId);
            var result  = await handler.HandleAsync(command, cancellationToken);
            if (!result.Success) return result.ToActionResult();
            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
