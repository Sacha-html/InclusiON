using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Requests.Institutions;
using InclusiON.Application.Constants;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses.Institutions;
namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestion de instituciones educativas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class InstitutionsController : ControllerBase
    {
        #region Queries

        /// <summary>
        /// Obtiene todas las instituciones educativas.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<InstitutionResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<InstitutionResponse>>>> GetInstitutions(
            [FromServices] IQueryHandler<GetInstitutionsQuery, ApiResponse<PagedResponse<InstitutionResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetInstitutionsQuery(page, pageSize, search, isActive);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Crea una nueva institucion educativa.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<InstitutionResponse>>> CreateInstitution(
            [FromBody] CreateInstitutionRequest request,
            [FromServices] ICommandHandler<CreateInstitutionCommand, ApiResponse<InstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateInstitutionCommand(
                request.Name,
                request.Address,
                request.Phone,
                request.Email);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Cambia el estado activo/inactivo de una institucion. Maquina de estados: rechaza transicion no-op y bloquea baja si tiene profesionales activos.
        /// </summary>
        [HttpPatch("{id:int}")]
        [Authorize(Policy = Permissions.GlobalAdmin)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<InstitutionResponse>>> PatchInstitutionStatus(
            int id,
            [FromBody] PatchStatusRequest request,
            [FromServices] ICommandHandler<PatchInstitutionStatusCommand, ApiResponse<InstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new PatchInstitutionStatusCommand(id, request.IsActive);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza una institucion educativa existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = Permissions.Institutions.Update)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<InstitutionResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<InstitutionResponse>>> UpdateInstitution(
            int id,
            [FromBody] UpdateInstitutionRequest request,
            [FromServices] ICommandHandler<UpdateInstitutionCommand, ApiResponse<InstitutionResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateInstitutionCommand(
                id,
                request.Name,
                request.Address,
                request.Phone,
                request.Email);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion
    }
}
