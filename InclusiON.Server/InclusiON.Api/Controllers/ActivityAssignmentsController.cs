using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Api.ModelBinders;
using InclusiON.Application.Authorization;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Activities;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InclusiON.Api.Controllers
{
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    public class ActivityAssignmentsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IResourceAuthorizationService _resourceAuthz;

        public ActivityAssignmentsController(
            IHttpContextService httpContextService,
            IResourceAuthorizationService resourceAuthz)
        {
            _httpContextService = httpContextService;
            _resourceAuthz      = resourceAuthz;
        }

        /// <summary>Obtiene una asignación por ID con ContentJson y TemplateTypeCode completos.</summary>
        [HttpGet("activity-assignments/{assignmentId}")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ActivityAssignmentResponse>>> GetAssignmentById(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int assignmentId,
            [FromServices] IQueryHandler<GetAssignmentByIdQuery, ApiResponse<ActivityAssignmentResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var requesterId = _httpContextService.GetCurrentEntityId();
            if (requesterId is null)
                return NotFound(ApiResponse<ActivityAssignmentResponse>.NotFound("Usuario"));

            var result = await handler.HandleAsync(
                new GetAssignmentByIdQuery(assignmentId, requesterId.Value),
                cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Asigna una actividad a una persona.</summary>
        [HttpPost("activity-assignments")]
        [Authorize(Policy = Permissions.Activities.Create)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ActivityAssignmentResponse>>> CreateAssignment(
            [FromBody] CreateActivityAssignmentRequest request,
            [FromServices] ICommandHandler<CreateActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityAssignmentResponse>.NotFound("Profesional"));

            if (!await _resourceAuthz.CanAccessPersonAsync(request.PersonId, AccessMode.Write, cancellationToken))
                return ApiResponse<ActivityAssignmentResponse>.Forbidden().ToActionResult();

            var command = new CreateActivityAssignmentCommand(
                request.EncryptedActivityId,
                request.PersonId,
                professionalId.Value,
                request.DueDate,
                request.IsEvaluationActivity,
                request.SequenceOrder,
                request.BypassDuplicateWarning);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(
                nameof(GetPersonAssignments),
                new { personId = request.PersonId },
                result);
        }

        /// <summary>Lista de asignaciones activas de una persona (vista estudiante).</summary>
        [HttpGet("persons/{personId}/activity-assignments")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityAssignmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityAssignmentResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<ActivityAssignmentResponse>>>> GetPersonAssignments(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var requesterId = _httpContextService.GetCurrentEntityId();
            if (requesterId is null)
                return NotFound(ApiResponse<List<ActivityAssignmentResponse>>.NotFound("Usuario"));

            var result = await handler.HandleAsync(
                new GetPersonActivityAssignmentsQuery(personId, requesterId.Value),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Asignaciones del estudiante autenticado (usa el entityId del JWT como personId).
        /// Evita que el cliente necesite conocer su propio GUID de persona.
        /// </summary>
        [HttpGet("my/activity-assignments")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityAssignmentResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityAssignmentResponse>>>> GetMyAssignments(
            [FromServices] IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var personId = _httpContextService.GetCurrentEntityId();
            if (personId is null)
                return NotFound(ApiResponse<List<ActivityAssignmentResponse>>.NotFound("Persona"));

            var result = await handler.HandleAsync(
                new GetPersonActivityAssignmentsQuery(personId.Value, personId.Value),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Inicia un intento de actividad (el estudiante arranca a jugar).</summary>
        [HttpPost("activity-assignments/{assignmentId}/responses/start")]
        [Authorize(Policy = Permissions.Activities.Respond)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ActivityAssignmentResponse>>> StartResponse(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int assignmentId,
            [FromServices] ICommandHandler<StartActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var personId = _httpContextService.GetCurrentEntityId();
            if (personId is null)
                return NotFound(ApiResponse<ActivityAssignmentResponse>.NotFound("Persona"));

            var result = await handler.HandleAsync(
                new StartActivityResponseCommand(assignmentId, personId.Value),
                cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Completa un intento de actividad con los resultados.</summary>
        [HttpPost("activity-assignments/{assignmentId}/responses/{responseId}/complete")]
        [Authorize(Policy = Permissions.Activities.Respond)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityAssignmentResponse>>> CompleteResponse(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int assignmentId,
            [ModelBinder(typeof(EncryptedIntModelBinder))] int responseId,
            [FromBody] CompleteActivityResponseRequest request,
            [FromServices] ICommandHandler<CompleteActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var personId = _httpContextService.GetCurrentEntityId();
            if (personId is null)
                return NotFound(ApiResponse<ActivityAssignmentResponse>.NotFound("Persona"));

            var command = new CompleteActivityResponseCommand(
                assignmentId,
                responseId,
                personId.Value,
                request.SuccessPercentage,
                request.TimeSpentSeconds,
                request.RequiredSupport,
                request.FrustrationLevel,
                request.ResponsePattern,
                request.Observations);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Cancela una asignación pendiente (solo el profesional que la creó).</summary>
        [HttpPatch("activity-assignments/{assignmentId}/cancel")]
        [Authorize(Policy = Permissions.Activities.Create)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityAssignmentResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityAssignmentResponse>>> CancelAssignment(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int assignmentId,
            [FromServices] ICommandHandler<CancelActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityAssignmentResponse>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new CancelActivityAssignmentCommand(assignmentId, professionalId.Value),
                cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }
    }
}
