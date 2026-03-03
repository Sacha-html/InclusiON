using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.DTOs.Requests.Auth;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Resources;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador de autenticacion y registro de usuarios.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
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
                return BadRequest(ApiResponse<UserResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
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

        /// <summary>
        /// Inicia sesion con email y contrasena.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
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

                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
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
                return BadRequest(ApiResponse<IdentifyUserResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
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
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status400BadRequest)]
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
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
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
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status400BadRequest)]
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
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
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
        [ProducesResponseType(typeof(ApiResponse<VisualLoginResponse>), StatusCodes.Status400BadRequest)]
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
                return BadRequest(ApiResponse<VisualLoginResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new AssistedLoginCommand(request.UserId, request.SupervisorEmail, request.SupervisorPassword, request.DeviceId);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success || result.Data?.Success == false)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        #endregion

        /// <summary>
        /// Refresca el token de acceso usando un refresh token valido.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            [FromServices] ICommandHandler<RefreshTokenCommand, ApiResponse<LoginResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

    }
}
