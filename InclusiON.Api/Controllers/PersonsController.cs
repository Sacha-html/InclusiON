using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.UseCases.Persons.Commands;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using System.Security.Claims;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PersonsController : ControllerBase
    {
        /// <summary>
        /// Actualiza el metodo de login de una persona con discapacidad.
        /// Solo el propio usuario o un supervisor autorizado puede realizar esta accion.
        /// </summary>
        /// <param name="userId">ID del usuario cuyo metodo de login se va a actualizar</param>
        /// <param name="request">Datos del nuevo metodo de login</param>
        [HttpPut("{userId:guid}/login-method")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UpdateLoginMethodResponse>>> UpdateLoginMethod(
            Guid userId,
            [FromBody] UpdateLoginMethodRequest request,
            [FromServices] ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UpdateLoginMethodResponse>.ErrorResult("Validation failed", errors));
            }

            // Verificar que el usuario autenticado tiene permiso
            var currentUserIdClaim = User.FindFirst("sub") ??
                User.FindFirst("userId") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("id");

            if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(ApiResponse<UpdateLoginMethodResponse>.ErrorResult("Token invalido"));
            }

            // Por ahora, permitimos que cualquier usuario autenticado actualice
            // En el futuro, se puede agregar logica para verificar si es el propio usuario
            // o un supervisor/profesional autorizado

            var command = new UpdateLoginMethodCommand(
                userId,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                if (result.Message.Contains("no encontrad", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Actualiza el metodo de login del usuario autenticado (persona con discapacidad).
        /// </summary>
        [HttpPut("me/login-method")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UpdateLoginMethodResponse>>> UpdateMyLoginMethod(
            [FromBody] UpdateLoginMethodRequest request,
            [FromServices] ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UpdateLoginMethodResponse>.ErrorResult("Validation failed", errors));
            }

            var userIdClaim = User.FindFirst("sub") ??
                User.FindFirst("userId") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("id");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(ApiResponse<UpdateLoginMethodResponse>.ErrorResult("Token invalido"));
            }

            var command = new UpdateLoginMethodCommand(
                userId,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                if (result.Message.Contains("no encontrad", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
