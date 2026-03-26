using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Requests.Assignments;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;
namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para asignaciones profesional-persona y profesional-institucion.
    /// </summary>
    [Route("api/professionals")]
    [ApiController]
    [Produces("application/json")]
    public class AssignmentsController : ControllerBase
    {
        #region Professional-Person Assignments

        /// <summary>
        /// Obtiene las personas asignadas a un profesional.
        /// </summary>
        [HttpGet("{professionalId:guid}/persons")]
        [Authorize(Policy = "persons:read")]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalPersonResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProfessionalPersonResponse>>>> GetPersonsByProfessional(
            Guid professionalId,
            [FromServices] IQueryHandler<GetPersonsByProfessionalQuery, ApiResponse<List<ProfessionalPersonResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPersonsByProfessionalQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Asigna una persona a un profesional.
        /// </summary>
        [HttpPost("{professionalId:guid}/persons")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ProfessionalPersonResponse>>> AssignPerson(
            Guid professionalId,
            [FromBody] AssignPersonRequest request,
            [FromServices] ICommandHandler<AssignPersonCommand, ApiResponse<ProfessionalPersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {

            var command = new AssignPersonCommand(
                professionalId,
                request.PersonId,
                request.IsPrimaryProfessional,
                request.CanSuperviseLogin);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva la asignacion de una persona a un profesional.
        /// </summary>
        [HttpPut("{professionalId:guid}/persons/{personId:guid}/deactivate")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalPersonResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfessionalPersonResponse>>> DeactivatePersonAssignment(
            Guid professionalId,
            Guid personId,
            [FromServices] ICommandHandler<DeactivatePersonAssignmentCommand, ApiResponse<ProfessionalPersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivatePersonAssignmentCommand(professionalId, personId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Professional-Institution Assignments

        /// <summary>
        /// Obtiene las instituciones asignadas a un profesional.
        /// </summary>
        [HttpGet("{professionalId:guid}/institutions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ProfessionalInstitutionResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ProfessionalInstitutionResponse>>>> GetInstitutionsByProfessional(
            Guid professionalId,
            [FromServices] IQueryHandler<GetInstitutionsByProfessionalQuery, ApiResponse<List<ProfessionalInstitutionResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetInstitutionsByProfessionalQuery(professionalId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Asigna una institucion a un profesional.
        /// </summary>
        [HttpPost("{professionalId:guid}/institutions")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ProfessionalInstitutionResponse>>> AssignInstitution(
            Guid professionalId,
            [FromBody] AssignInstitutionRequest request,
            [FromServices] ICommandHandler<AssignInstitutionCommand, ApiResponse<ProfessionalInstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new AssignInstitutionCommand(
                professionalId,
                request.InstitutionId);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Remueve la asignacion de una institucion a un profesional.
        /// </summary>
        [HttpDelete("{professionalId:guid}/institutions/{institutionId:int}")]
        [Authorize(Policy = "professionals:update")]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProfessionalInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProfessionalInstitutionResponse>>> RemoveInstitutionAssignment(
            Guid professionalId,
            int institutionId,
            [FromServices] ICommandHandler<RemoveInstitutionAssignmentCommand, ApiResponse<ProfessionalInstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new RemoveInstitutionAssignmentCommand(professionalId, institutionId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion
    }
}
