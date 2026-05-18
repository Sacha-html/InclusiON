using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Assignments;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Api.Controllers
{
    [Route("api/professionals")]
    [ApiController]
    [Produces("application/json")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;
        private readonly IResourceAuthorizationService _resourceAuthz;

        public AssignmentsController(
            IHttpContextService httpContextService,
            IResourceAuthorizationService resourceAuthz)
        {
            _httpContextService = httpContextService;
            _resourceAuthz      = resourceAuthz;
        }

        #region Professional-Person Assignments

        [HttpGet("{professionalId}/persons")]
        [Authorize(Policy = "persons:read")]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalPersonResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<ProfessionalPersonResponse>>>> GetPersonsByProfessional(
            Guid professionalId,
            [FromServices] IQueryHandler<GetPersonsByProfessionalQuery, ApiResponse<List<ProfessionalPersonResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<List<ProfessionalPersonResponse>>.Forbidden().ToActionResult();

            var query = new GetPersonsByProfessionalQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{professionalId}/persons")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ProfessionalPersonResponse>>> AssignPerson(
            Guid professionalId,
            [FromBody] AssignPersonRequest request,
            [FromServices] ICommandHandler<AssignPersonCommand, ApiResponse<ProfessionalPersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!await _resourceAuthz.CanAccessPersonAsync(request.PersonId, AccessMode.Write, cancellationToken))
                return ApiResponse<ProfessionalPersonResponse>.Forbidden().ToActionResult();

            var command = new AssignPersonCommand(
                professionalId,
                request.PersonId,
                request.IsPrimaryProfessional,
                request.CanSuperviseLogin);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{professionalId}/persons/{personId}/deactivate")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfessionalPersonResponse>>> DeactivatePersonAssignment(
            Guid professionalId,
            Guid personId,
            [FromServices] ICommandHandler<DeactivatePersonAssignmentCommand, ApiResponse<ProfessionalPersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!await _resourceAuthz.CanAccessPersonAsync(personId, AccessMode.Write, cancellationToken))
                return ApiResponse<ProfessionalPersonResponse>.Forbidden().ToActionResult();

            var command = new DeactivatePersonAssignmentCommand(professionalId, personId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Professional-Institution Assignments

        [HttpGet("{professionalId}/institutions")]
        [Authorize(Policy = "professionals:read")]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalInstitutionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<ProfessionalInstitutionResponse>>>> GetInstitutionsByProfessional(
            Guid professionalId,
            [FromServices] IQueryHandler<GetInstitutionsByProfessionalQuery, ApiResponse<List<ProfessionalInstitutionResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId();
            if (entityId.HasValue && entityId.Value != professionalId)
                return ApiResponse<List<ProfessionalInstitutionResponse>>.Forbidden().ToActionResult();

            var query = new GetInstitutionsByProfessionalQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{professionalId}/institutions")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ProfessionalInstitutionResponse>>> AssignInstitution(
            Guid professionalId,
            [FromBody] AssignInstitutionRequest request,
            [FromServices] ICommandHandler<AssignInstitutionCommand, ApiResponse<ProfessionalInstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var institutionIds = _httpContextService.GetInstitutionIds();
            if (institutionIds.Count > 0 && !institutionIds.Contains(request.InstitutionId))
                return ApiResponse<ProfessionalInstitutionResponse>.Forbidden().ToActionResult();

            var command = new AssignInstitutionCommand(
                professionalId,
                request.InstitutionId);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{professionalId}/institutions/{institutionId:int}")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfessionalInstitutionResponse>>> RemoveInstitutionAssignment(
            Guid professionalId,
            int institutionId,
            [FromServices] ICommandHandler<RemoveInstitutionAssignmentCommand, ApiResponse<ProfessionalInstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var institutionIds = _httpContextService.GetInstitutionIds();
            if (institutionIds.Count > 0 && !institutionIds.Contains(institutionId))
                return ApiResponse<ProfessionalInstitutionResponse>.Forbidden().ToActionResult();

            var command = new RemoveInstitutionAssignmentCommand(professionalId, institutionId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion
    }
}
