using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CatalogsController : ControllerBase
    {
        [HttpGet("disability-types")]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<CatalogItemResponse>>>> GetDisabilityTypes(
            [FromServices] IQueryHandler<GetDisabilityTypesQuery, ApiResponse<List<CatalogItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetDisabilityTypesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("autonomy-levels")]
        [ProducesResponseType(typeof(ApiResponse<List<AutonomyLevelResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AutonomyLevelResponse>>>> GetAutonomyLevels(
            [FromServices] IQueryHandler<GetAutonomyLevelsQuery, ApiResponse<List<AutonomyLevelResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetAutonomyLevelsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("activity-categories")]
        [ProducesResponseType(typeof(ApiResponse<List<CatalogItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<CatalogItemResponse>>>> GetActivityCategories(
            [FromServices] IQueryHandler<GetActivityCategoriesQuery, ApiResponse<List<CatalogItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetActivityCategoriesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("skill-areas")]
        [ProducesResponseType(typeof(ApiResponse<List<SkillAreaResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<SkillAreaResponse>>>> GetSkillAreas(
            [FromServices] IQueryHandler<GetSkillAreasQuery, ApiResponse<List<SkillAreaResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetSkillAreasQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("activity-template-types")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityTemplateTypeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityTemplateTypeResponse>>>> GetActivityTemplateTypes(
            [FromServices] IQueryHandler<GetActivityTemplateTypesQuery, ApiResponse<List<ActivityTemplateTypeResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetActivityTemplateTypesQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("login-methods")]
        [ProducesResponseType(typeof(ApiResponse<List<LoginMethodResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<LoginMethodResponse>>>> GetLoginMethods(
            [FromServices] IQueryHandler<GetLoginMethodsQuery, ApiResponse<List<LoginMethodResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetLoginMethodsQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
