using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Requests.Roadmap;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestion del roadmap personalizado de una persona.
    /// </summary>
    [Route("api/persons/{personId}/roadmap")]
    [ApiController]
    [Produces("application/json")]
    public class RoadmapController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public RoadmapController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        // ────────────────────────────────────────────────────────────────
        // Queries
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el roadmap completo de una persona (areas y actividades).
        /// </summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Roadmap.Read)]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoadmapResponse>>> GetRoadmap(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonRoadmapQuery(personId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene el roadmap de la persona autenticada (usa entityId del JWT).
        /// Usado por el portal AAC para evitar exponer el personId en el cliente.
        /// </summary>
        [HttpGet("/api/my/roadmap")]
        [Authorize(Policy = Permissions.Roadmap.Read)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoadmapResponse>>> GetMyRoadmap(
            [FromServices] IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var personId = _httpContextService.GetCurrentEntityId();
            if (personId is null)
                return NotFound(ApiResponse<RoadmapResponse>.NotFound("Persona"));

            var result = await handler.HandleAsync(new GetPersonRoadmapQuery(personId.Value), cancellationToken);
            return result.ToActionResult();
        }

        // ────────────────────────────────────────────────────────────────
        // Roadmap CRUD
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea el roadmap de una persona. Solo puede existir un roadmap por persona.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Roadmap.Create)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoadmapResponse>>> CreateRoadmap(
            Guid personId,
            [FromBody] CreateRoadmapRequest request,
            [FromServices] ICommandHandler<CreateRoadmapCommand, ApiResponse<RoadmapResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<RoadmapResponse>.NotFound("Profesional"));

            var command = new CreateRoadmapCommand(personId, professionalId.Value, request.Notes);
            var result  = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetRoadmap), new { personId }, result);
        }

        /// <summary>
        /// Actualiza las notas del roadmap de una persona.
        /// </summary>
        [HttpPatch("notes")]
        [Authorize(Policy = Permissions.Roadmap.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<RoadmapResponse>>> UpdateNotes(
            Guid personId,
            [FromBody] UpdateRoadmapNotesRequest request,
            [FromServices] ICommandHandler<UpdateRoadmapNotesCommand, ApiResponse<RoadmapResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(
                new UpdateRoadmapNotesCommand(personId, request.Notes), cancellationToken);
            return result.ToActionResult();
        }

        // ────────────────────────────────────────────────────────────────
        // Areas
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Agrega un area de habilidad al roadmap de una persona.
        /// </summary>
        [HttpPost("areas")]
        [Authorize(Policy = Permissions.Roadmap.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapAreaResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapAreaResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoadmapAreaResponse>>> AddArea(
            Guid personId,
            [FromBody] AddRoadmapAreaRequest request,
            [FromServices] ICommandHandler<AddRoadmapAreaCommand, ApiResponse<RoadmapAreaResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new AddRoadmapAreaCommand(personId, request.SkillAreaId, request.DisplayOrder);
            var result  = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Elimina un area del roadmap de una persona (con sus actividades en cascada).
        /// </summary>
        [HttpDelete("areas/{areaId:int}")]
        [Authorize(Policy = Permissions.Roadmap.Delete)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveArea(
            Guid personId,
            int areaId,
            [FromServices] ICommandHandler<RemoveRoadmapAreaCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new RemoveRoadmapAreaCommand(areaId), cancellationToken);
            return result.ToActionResult();
        }

        // ────────────────────────────────────────────────────────────────
        // Actividades
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Agrega una actividad a un area del roadmap.
        /// </summary>
        [HttpPost("areas/{areaId:int}/activities")]
        [Authorize(Policy = Permissions.Roadmap.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoadmapActivityResponse>>> AddActivity(
            Guid personId,
            int areaId,
            [FromBody] AddRoadmapActivityRequest request,
            [FromServices] ICommandHandler<AddRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<RoadmapActivityResponse>.NotFound("Profesional"));

            var command = new AddRoadmapActivityCommand(
                areaId,
                request.ActivityId,
                professionalId.Value,
                request.SequenceOrder,
                request.UnlockThresholdPercent,
                request.TimeLimitSeconds,
                request.MaxAttempts,
                request.ShowHints,
                request.DifficultyLevel);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Reordena las actividades de un area del roadmap.
        /// </summary>
        [HttpPut("areas/{areaId:int}/activities/reorder")]
        [Authorize(Policy = Permissions.Roadmap.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ReorderActivities(
            Guid personId,
            int areaId,
            [FromBody] ReorderRoadmapActivitiesRequest request,
            [FromServices] ICommandHandler<ReorderRoadmapActivitiesCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var items = request.Activities
                .Select(a => (a.Id, a.SequenceOrder))
                .ToList();

            var result = await handler.HandleAsync(
                new ReorderRoadmapActivitiesCommand(areaId, items), cancellationToken);

            return result.ToActionResult();
        }

        /// <summary>
        /// Elimina una actividad del roadmap.
        /// </summary>
        [HttpDelete("areas/{areaId:int}/activities/{activityEntryId:int}")]
        [Authorize(Policy = Permissions.Roadmap.Delete)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveActivity(
            Guid personId,
            int areaId,
            int activityEntryId,
            [FromServices] ICommandHandler<RemoveRoadmapActivityCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(
                new RemoveRoadmapActivityCommand(activityEntryId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desbloquea manualmente una actividad del roadmap.
        /// </summary>
        [HttpPut("areas/{areaId:int}/activities/{activityEntryId:int}/unlock")]
        [Authorize(Policy = Permissions.Roadmap.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<RoadmapActivityResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RoadmapActivityResponse>>> UnlockActivity(
            Guid personId,
            int areaId,
            int activityEntryId,
            [FromServices] ICommandHandler<UnlockRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(
                new UnlockRoadmapActivityCommand(activityEntryId), cancellationToken);
            return result.ToActionResult();
        }
    }
}
