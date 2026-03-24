using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Family;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class FamilyController : ControllerBase
    {
        #region Queries

        [HttpGet]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<FamilyListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<FamilyListItemResponse>>>> GetFamily(
            [FromQuery] GetFamilyRequest request,
            [FromServices] IQueryHandler<GetFamilyQuery, ApiResponse<PagedResponse<FamilyListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var query = new GetFamilyQuery(
                request.Page, request.PageSize, request.Search, request.IsActive,
                request.SortBy, request.SortDirection, request.InstitutionId);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{familyId:guid}")]
        [Authorize(Policy = "family:read")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> GetFamilyById(
            Guid familyId,
            [FromServices] IQueryHandler<GetFamilyByIdQuery, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetFamilyByIdQuery(familyId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Commands

        [HttpPost]
        [Authorize(Policy = "family:create")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> CreateFamily(
            [FromBody] CreateFamilyRequest request,
            [FromServices] ICommandHandler<CreateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {

            var command = new CreateFamilyCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.DocumentNumber,
                request.Phone,
                request.Relationship);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetFamilyById),
                new { familyId = result.Data!.Id },
                result);
        }

        [HttpPut("{familyId:guid}")]
        [Authorize(Policy = "family:update")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> UpdateFamily(
            Guid familyId,
            [FromBody] UpdateFamilyRequest request,
            [FromServices] ICommandHandler<UpdateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {

            var command = new UpdateFamilyCommand(
                familyId,
                request.FirstName,
                request.LastName,
                request.Email,
                request.DocumentNumber,
                request.Phone,
                request.Relationship);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{familyId:guid}/deactivate")]
        [Authorize(Policy = "family:delete")]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FamilyResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FamilyResponse>>> DeactivateFamily(
            Guid familyId,
            [FromServices] ICommandHandler<DeactivateFamilyCommand, ApiResponse<FamilyResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivateFamilyCommand(familyId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion
    }
}
