using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador de catalogos del sistema.
    /// Provee endpoints de solo lectura para listas de referencia.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [ResponseCache(Duration = 300)]
    [OutputCache(PolicyName = "catalogs")]
    public class CatalogsController : ControllerBase
    {
        /// <summary>
        /// Obtiene los tipos de discapacidad activos.
        /// </summary>
        [HttpGet("disability-types")]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<CatalogItemResponse>>>> GetDisabilityTypes(
            [FromServices] IQueryHandler<GetDisabilityTypesQuery, ApiResponse<List<CatalogItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetDisabilityTypesQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los niveles de autonomia activos.
        /// </summary>
        [HttpGet("autonomy-levels")]
        [ProducesResponseType(typeof(ApiResponse<List<AutonomyLevelResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AutonomyLevelResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<AutonomyLevelResponse>>>> GetAutonomyLevels(
            [FromServices] IQueryHandler<GetAutonomyLevelsQuery, ApiResponse<List<AutonomyLevelResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetAutonomyLevelsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las categorias de actividades activas.
        /// </summary>
        [HttpGet("activity-categories")]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<CatalogItemResponse>>>> GetActivityCategories(
            [FromServices] IQueryHandler<GetActivityCategoriesQuery, ApiResponse<List<CatalogItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetActivityCategoriesQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene las areas de habilidad activas.
        /// </summary>
        [HttpGet("skill-areas")]
        [ProducesResponseType(typeof(ApiResponse<List<SkillAreaResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<SkillAreaResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<SkillAreaResponse>>>> GetSkillAreas(
            [FromServices] IQueryHandler<GetSkillAreasQuery, ApiResponse<List<SkillAreaResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetSkillAreasQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los tipos de plantilla de actividad activos.
        /// </summary>
        [HttpGet("activity-template-types")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityTemplateTypeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityTemplateTypeResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<ActivityTemplateTypeResponse>>>> GetActivityTemplateTypes(
            [FromServices] IQueryHandler<GetActivityTemplateTypesQuery, ApiResponse<List<ActivityTemplateTypeResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetActivityTemplateTypesQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los metodos de login activos.
        /// </summary>
        [HttpGet("login-methods")]
        [ProducesResponseType(typeof(ApiResponse<List<LoginMethodResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LoginMethodResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<LoginMethodResponse>>>> GetLoginMethods(
            [FromServices] IQueryHandler<GetLoginMethodsQuery, ApiResponse<List<LoginMethodResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetLoginMethodsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los colores disponibles para avatares de usuarios.
        /// </summary>
        [HttpGet("avatar-colors")]
        [ProducesResponseType(typeof(ApiResponse<List<AvatarColorResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AvatarColorResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<AvatarColorResponse>>>> GetAvatarColors(
            [FromServices] IQueryHandler<GetAvatarColorsQuery, ApiResponse<List<AvatarColorResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetAvatarColorsQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
