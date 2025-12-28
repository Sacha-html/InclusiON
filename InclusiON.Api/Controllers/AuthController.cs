using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.ApplicationBusiness.UseCases.Users.Queries;
using InclusiON.DTOs.Requests.Auth;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using System.Security.Claims;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Register(
            [FromBody] RegisterRequest request,
            [FromServices] ICommandHandler<RegisterUserCommand, ApiResponse<UserResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new RegisterUserCommand(
                request.Name,
                request.Surname,
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.PhoneNumber);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Created($"api/auth/profile", result);
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
            [FromBody] LoginRequest request,
            [FromServices] ICommandHandler<LoginCommand, ApiResponse<LoginResponse>> handler,
            CancellationToken cancellationToken
            )
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new LoginCommand(request.Email, request.Password);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        [HttpGet("profile")]
        [Authorize] // ✅ Requiere autenticación JWT
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile(
            [FromServices] IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userIdCaim = User.FindFirst("sub") ??
                    User.FindFirst("userId") ??
                    User.FindFirst(ClaimTypes.NameIdentifier) ??
                    User.FindFirst("id");

                if (userIdCaim is null)
                {
                    return Unauthorized(ApiResponse<UserProfileResponse>.ErrorResult("Invalid Token"));
                }

                if (!Guid.TryParse(userIdCaim.Value, out Guid userId))
                {
                    return Unauthorized(ApiResponse<UserProfileResponse>.ErrorResult(
                        "Invalid token - user ID format is invalid"));
                }

                var query = new GetUserProfileQuery(userId);
                var result = await handler.HandleAsync(query, cancellationToken);

                if (!result.Success)
                {
                    if(result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }

                    if (result.Message.Contains("deactivated", StringComparison.OrdinalIgnoreCase))
                    {
                        return Unauthorized(result);
                    }

                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                return BadRequest(ApiResponse<UserProfileResponse>.ErrorResult("Operation was cancelled"));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<UserProfileResponse>.ErrorResult("Internal server error occurred"));
            }
        }
    }
}