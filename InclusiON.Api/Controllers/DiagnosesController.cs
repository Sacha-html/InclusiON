using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Diagnoses.Commands;
using InclusiON.Application.UseCases.Diagnoses.Queries;
using InclusiON.DTOs.Requests.Diagnoses;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Diagnoses;

namespace InclusiON.Api.Controllers
{
    [Route("api")]
    [ApiController]
    [Produces("application/json")]
    public class DiagnosesController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public DiagnosesController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        [HttpGet("persons/{personId:guid}/diagnoses")]
        [Authorize(Policy = "diagnoses:read")]
        [ProducesResponseType(typeof(ApiResponse<List<DiagnosisListItemResponse>>), StatusCodes.Status200OK)]
        [PersonAccess(AccessMode.Read)]
        public async Task<ActionResult<ApiResponse<List<DiagnosisListItemResponse>>>> GetDiagnoses(
            Guid personId,
            [FromServices] IQueryHandler<GetDiagnosesQuery, ApiResponse<List<DiagnosisListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetDiagnosesQuery(personId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("diagnoses/{id:int}")]
        [Authorize(Policy = "diagnoses:read")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status404NotFound)]
        [DiagnosisAccess(AccessMode.Read)]
        public async Task<ActionResult<ApiResponse<DiagnosisResponse>>> GetDiagnosisById(
            int id,
            [FromServices] IQueryHandler<GetDiagnosisByIdQuery, ApiResponse<DiagnosisResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetDiagnosisByIdQuery(id);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("persons/{personId:guid}/diagnoses")]
        [Authorize(Policy = "diagnoses:create")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status404NotFound)]
        [PersonAccess(AccessMode.Write)]
        public async Task<ActionResult<ApiResponse<DiagnosisResponse>>> CreateDiagnosis(
            Guid personId,
            [FromBody] CreateDiagnosisRequest request,
            [FromServices] ICommandHandler<CreateDiagnosisCommand, ApiResponse<DiagnosisResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<DiagnosisResponse>.ErrorResult("Solo los profesionales pueden crear diagnósticos."));

            var command = new CreateDiagnosisCommand(
                personId,
                professionalId.Value,
                request.DiagnosisDate,
                request.PrimaryDiagnosis,
                request.InitialObservations,
                request.IdentifiedCapabilities,
                request.IdentifiedChallenges,
                request.RequiredSupports,
                request.PedagogicalObjectives,
                request.RecommendedStrategies);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("diagnoses/{id:int}")]
        [Authorize(Policy = "diagnoses:update")]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<DiagnosisResponse>), StatusCodes.Status404NotFound)]
        [DiagnosisAccess(AccessMode.Write)]
        public async Task<ActionResult<ApiResponse<DiagnosisResponse>>> UpdateDiagnosis(
            int id,
            [FromBody] UpdateDiagnosisRequest request,
            [FromServices] ICommandHandler<UpdateDiagnosisCommand, ApiResponse<DiagnosisResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return BadRequest(ApiResponse<DiagnosisResponse>.ErrorResult("Solo los profesionales pueden editar diagnósticos."));

            var command = new UpdateDiagnosisCommand(
                id,
                professionalId.Value,
                request.DiagnosisDate,
                request.PrimaryDiagnosis,
                request.InitialObservations,
                request.IdentifiedCapabilities,
                request.IdentifiedChallenges,
                request.RequiredSupports,
                request.PedagogicalObjectives,
                request.RecommendedStrategies);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }
    }
}
