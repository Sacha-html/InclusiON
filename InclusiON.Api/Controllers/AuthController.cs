using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.UseCases.Auth.Commands;
using InclusiON.ApplicationBusiness.UseCases.Auth.Queries;
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

        /// <summary>
        /// Obtiene los metodos de login disponibles (activos) para personas con discapacidad.
        /// </summary>
        [HttpGet("login-methods")]
        [ProducesResponseType(typeof(ApiResponse<List<LoginMethodResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<LoginMethodResponse>>>> GetLoginMethods(
            [FromServices] IQueryHandler<GetLoginMethodsQuery, ApiResponse<List<LoginMethodResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetLoginMethodsQuery();
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        #region Visual Login Methods

        /// <summary>
        /// Identifica un usuario antes del login para obtener su metodo de autenticacion.
        /// </summary>
        [HttpPost("identify")]
        [ProducesResponseType(typeof(ApiResponse<IdentifyUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IdentifyUserResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IdentifyUserResponse>>> IdentifyUser(
            [FromBody] IdentifyUserRequest request,
            [FromServices] IQueryHandler<IdentifyUserQuery, ApiResponse<IdentifyUserResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<IdentifyUserResponse>.ErrorResult("Validation failed", errors));
            }

            var query = new IdentifyUserQuery(request.Identifier, request.DeviceId, request.UserType);
            var result = await handler.HandleAsync(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Login visual estandar con contrasena para personas con discapacidad.
        /// </summary>
        [HttpPost("login/visual-standard")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginVisualStandard(
            [FromBody] VisualStandardLoginRequest request,
            [FromServices] ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new VisualStandardLoginCommand(request.UserId, request.Password, request.DeviceId, request.RememberDevice);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login con PIN numerico para personas con discapacidad.
        /// </summary>
        [HttpPost("login/pin")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginWithPin(
            [FromBody] PinLoginRequest request,
            [FromServices] ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new PinLoginCommand(request.UserId, request.Pin, request.DeviceId, request.RememberDevice);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login asistido donde un familiar o profesional autoriza el acceso.
        /// </summary>
        [HttpPost("login/assisted")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginAssisted(
            [FromBody] AssistedLoginRequest request,
            [FromServices] ICommandHandler<AssistedLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new AssistedLoginCommand(request.UserId, request.SupervisorEmail, request.SupervisorPassword, request.DeviceId);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        #region Deprecated Visual Login Methods

        /// <summary>
        /// Login con secuencia de emojis para personas con discapacidad.
        /// </summary>
        [Obsolete("Este metodo de login ha sido deprecado. Use PIN o Login Asistido en su lugar.")]
        [HttpPost("login/emoji")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginWithEmoji(
            [FromBody] EmojiLoginRequest request,
            [FromServices] ICommandHandler<EmojiLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new EmojiLoginCommand(request.UserId, request.EmojiSequence, request.DeviceId, request.RememberDevice);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login con seleccion de color y forma para personas con discapacidad.
        /// </summary>
        [Obsolete("Este metodo de login ha sido deprecado. Use PIN o Login Asistido en su lugar.")]
        [HttpPost("login/color-shape")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginWithColorShape(
            [FromBody] ColorShapeLoginRequest request,
            [FromServices] ICommandHandler<ColorShapeLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new ColorShapeLoginCommand(request.UserId, request.ColorShapeId, request.DeviceId, request.RememberDevice);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login automatico desde dispositivo confiable.
        /// </summary>
        [Obsolete("Este metodo de login ha sido deprecado. Use PIN o Login Asistido en su lugar.")]
        [HttpPost("login/trusted-device")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginWithTrustedDevice(
            [FromBody] TrustedDeviceLoginRequest request,
            [FromServices] ICommandHandler<TrustedDeviceLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new TrustedDeviceLoginCommand(request.UserId, request.DeviceId, request.DeviceToken);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login por seleccion de perfil visual.
        /// </summary>
        [Obsolete("Este metodo de login ha sido deprecado. Use PIN o Login Asistido en su lugar.")]
        [HttpPost("login/profile-select")]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<VisualLoginResponse>>> LoginWithProfileSelect(
            [FromBody] ProfileSelectLoginRequest request,
            [FromServices] ICommandHandler<ProfileSelectLoginCommand, ApiResponse<VisualLoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult("Validation failed", errors));
            }

            var command = new ProfileSelectLoginCommand(
                request.UserId,
                request.DeviceId,
                request.RequiresConfirmation,
                request.ConfirmationPin);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        #endregion

        #endregion

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