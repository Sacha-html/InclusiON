using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Admin;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Api.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Produces("application/json")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public AdminUsersController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        [HttpGet]
        [Authorize(Policy = "users:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<AdminUserListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<AdminUserListItemResponse>>>> GetUsers(
            [FromQuery] GetAdminUsersRequest request,
            [FromServices] IQueryHandler<GetAdminUsersQuery, ApiResponse<PagedResponse<AdminUserListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var query = new GetAdminUsersQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.Role,
                request.IsActive,
                request.SortBy,
                request.SortDirection,
                request.InstitutionIds);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = "users:read")]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminUserDetailResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminUserDetailResponse>>> GetUserDetail(
            Guid userId,
            [FromServices] IQueryHandler<GetAdminUserDetailQuery, ApiResponse<AdminUserDetailResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAdminUserDetailQuery(userId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("{userId}/activity")]
        [Authorize(Policy = "users:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserRecentSessionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserRecentSessionResponse>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserRecentSessionResponse>>>> GetUserActivity(
            Guid userId,
            [FromServices] IQueryHandler<GetUserActivityQuery, ApiResponse<PagedResponse<UserRecentSessionResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            CancellationToken cancellationToken = default)
        {
            var query = new GetUserActivityQuery(userId, page, pageSize);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{userId}/reset-password")]
        [Authorize(Policy = "users:update")]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResultResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ResetPasswordResultResponse>>> ResetPassword(
            Guid userId,
            [FromServices] ICommandHandler<AdminResetPasswordCommand, ApiResponse<ResetPasswordResultResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _httpContextService.GetCurrentUserId();
            if (currentUserId is null) return Unauthorized(ApiResponse<ResetPasswordResultResponse>.Unauthorized());
            var command = new AdminResetPasswordCommand(userId, currentUserId.Value);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{userId}")]
        [Authorize(Policy = "users:update")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUser(
            Guid userId,
            [FromBody] UpdateAdminUserRequest request,
            [FromServices] ICommandHandler<AdminUpdateUserCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _httpContextService.GetCurrentUserId();
            if (currentUserId is null) return Unauthorized(ApiResponse<object>.Unauthorized());
            var command = new AdminUpdateUserCommand(userId, currentUserId.Value, request.Name, request.Surname, request.Email);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{userId}/deactivate")]
        [Authorize(Policy = "users:delete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(
            Guid userId,
            [FromServices] ICommandHandler<AdminDeactivateUserCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _httpContextService.GetCurrentUserId();
            if (currentUserId is null) return Unauthorized(ApiResponse<object>.Unauthorized());
            var command = new AdminDeactivateUserCommand(userId, currentUserId.Value);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{userId}/reactivate")]
        [Authorize(Policy = "users:update")]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResultResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ResetPasswordResultResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ResetPasswordResultResponse>>> ReactivateUser(
            Guid userId,
            [FromServices] ICommandHandler<AdminReactivateUserCommand, ApiResponse<ResetPasswordResultResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _httpContextService.GetCurrentUserId();
            if (currentUserId is null) return Unauthorized(ApiResponse<ResetPasswordResultResponse>.Unauthorized());
            var command = new AdminReactivateUserCommand(userId, currentUserId.Value);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }
    }
}
