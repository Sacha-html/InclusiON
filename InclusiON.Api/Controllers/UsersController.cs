using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.UseCases.Users.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using System.Security.Claims;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestion de usuarios del sistema.
    /// Maneja operaciones sobre la cuenta del usuario autenticado (cualquier rol)
    /// y la administracion de usuarios (admin).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        #region Queries

        /// <summary>
        /// Obtiene el perfil del usuario autenticado a partir de los claims del token JWT.
        /// Funciona para cualquier rol: admin, profesional, familiar o persona.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMyProfile(
            [FromServices] IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<UserProfileResponse>.ErrorResult("Token invalido"));
            }

            var query = new GetUserProfileQuery(userId.Value);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Helpers

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ??
                User.FindFirst("userId") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("id");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return null;
            }

            return userId;
        }

        #endregion
    }
}
