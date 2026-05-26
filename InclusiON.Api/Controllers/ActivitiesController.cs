using InclusiON.Api.Extensions;
using InclusiON.Api.ModelBinders;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Activities;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using InclusiON.DTOs.Responses.Persons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public ActivitiesController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        /// <summary>Búsqueda semántica de actividades por texto libre.</summary>
        [HttpGet("search")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> SearchActivitiesSemantic(
            [FromQuery] string text,
            [FromServices] IQueryHandler<SearchActivitiesSemanticQuery, ApiResponse<List<ActivityListItemResponse>>> handler,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(ApiResponse<List<ActivityListItemResponse>>.ErrorResult(
                    ErrorCode.InvalidInput, "El texto de búsqueda es requerido."));

            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<ActivityListItemResponse>>.NotFound("Profesional"));

            limit = Math.Clamp(limit, 1, 50);

            var result = await handler.HandleAsync(
                new SearchActivitiesSemanticQuery(professionalId.Value, text, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Obtiene actividades similares a una actividad existente.</summary>
        [HttpGet("{id}/similar")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> GetSimilarActivities(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetSimilarActivitiesQuery, ApiResponse<List<ActivityListItemResponse>>> handler,
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<ActivityListItemResponse>>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new GetSimilarActivitiesQuery(professionalId.Value, id, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Obtiene personas compatibles con una actividad (ordenadas por compatibilidad).</summary>
        [HttpGet("{id}/compatible-persons")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonListItemResponse>>>> GetCompatiblePersons(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetCompatiblePersonsQuery, ApiResponse<List<PersonListItemResponse>>> handler,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<PersonListItemResponse>>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new GetCompatiblePersonsQuery(id, professionalId.Value, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Lista paginada de actividades (propias + estándar).</summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ActivityListItemResponse>>>> GetActivities(
            [FromQuery] GetActivitiesRequest request,
            [FromServices] IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<PagedResponse<ActivityListItemResponse>>.NotFound("Profesional"));

            request.Validate();

            var query = new GetActivitiesQuery(
                professionalId.Value,
                request.Search,
                request.CategoryId,
                request.SkillAreaId,
                request.TemplateTypeId,
                request.IsActive,
                request.IsStandard,
                request.Page,
                request.PageSize);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>Detalle de una actividad por ID.</summary>
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> GetActivity(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetActivityByIdQuery, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var result = await handler.HandleAsync(new GetActivityByIdQuery(id, professionalId.Value), cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Crea una nueva actividad.</summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Activities.Create)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> CreateActivity(
            [FromBody] CreateActivityRequest request,
            [FromServices] ICommandHandler<CreateActivityCommand, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var command = new CreateActivityCommand(
                professionalId.Value,
                request.Title,
                request.Description,
                request.Instructions,
                request.CategoryId,
                request.SkillAreaId,
                request.ComplexityLevel,
                request.EstimatedDurationMinutes,
                request.RequiresSupervision,
                request.HasVisualSupport,
                request.HasAudioSupport,
                request.UsesEasyReading,
                request.UsesPictograms,
                request.ResourcesUrl,
                request.TemplateTypeId,
                request.ContentJson);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetActivity), new { id = result.Data!.Id }, result);
        }

        /// <summary>Actualiza una actividad existente.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Permissions.Activities.Update)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> UpdateActivity(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromBody] UpdateActivityRequest request,
            [FromServices] ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var command = new UpdateActivityCommand(
                id,
                professionalId.Value,
                request.Title,
                request.Description,
                request.Instructions,
                request.CategoryId,
                request.SkillAreaId,
                request.ComplexityLevel,
                request.EstimatedDurationMinutes,
                request.RequiresSupervision,
                request.HasVisualSupport,
                request.HasAudioSupport,
                request.UsesEasyReading,
                request.UsesPictograms,
                request.ResourcesUrl,
                request.ContentJson);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Activa o da de baja una actividad (máquina de estados).</summary>
        [HttpPatch("{id}")]
        [Authorize(Policy = Permissions.Activities.Update)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> PatchActivityStatus(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromBody] PatchStatusRequest request,
            [FromServices] ICommandHandler<PatchActivityStatusCommand, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new PatchActivityStatusCommand(id, professionalId.Value, request.IsActive),
                cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }
    }
}
