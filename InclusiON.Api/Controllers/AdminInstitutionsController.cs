using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Admin;
using InclusiON.DTOs.Requests.Assignments;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Controllers
{
    [Route("api/admin/institutions-assignments")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = "settings:update")]
    public class AdminInstitutionsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public AdminInstitutionsController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        [HttpGet("admins")]
        [Authorize(Policy = "global-admin")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminUserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminUserResponse>>>> GetAllAdmins(
            [FromServices] IQueryHandler<GetAllAdminsQuery, ApiResponse<List<AdminUserResponse>>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new GetAllAdminsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<AdminInstitutionResponse>>>> GetMyInstitutions(
            [FromServices] IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<List<AdminInstitutionResponse>>> handler,
            CancellationToken cancellationToken)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null) return Unauthorized(ApiResponse<List<AdminInstitutionResponse>>.Unauthorized());

            var result = await handler.HandleAsync(new GetAdminInstitutionsQuery(userId.Value), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{adminUserId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminInstitutionResponse>>>> GetAdminInstitutions(
            Guid adminUserId,
            [FromServices] IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<List<AdminInstitutionResponse>>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(new GetAdminInstitutionsQuery(adminUserId), cancellationToken);
            return Ok(result);
        }

        [HttpPost("{adminUserId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> AssignInstitution(
            Guid adminUserId,
            [FromBody] AssignInstitutionRequest request,
            [FromServices] ICommandHandler<AssignInstitutionToAdminCommand, ApiResponse<AdminInstitutionResponse>> handler,
            CancellationToken cancellationToken)
        {
            var command = new AssignInstitutionToAdminCommand(adminUserId, request.InstitutionId);
            var result  = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{adminUserId:guid}/{institutionId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> RemoveAssignment(
            Guid adminUserId,
            int institutionId,
            [FromServices] ICommandHandler<RemoveAdminInstitutionCommand, ApiResponse<AdminInstitutionResponse>> handler,
            CancellationToken cancellationToken)
        {
            var command = new RemoveAdminInstitutionCommand(adminUserId, institutionId);
            var result  = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("users")]
        [Authorize(Policy = "global-admin")]
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
