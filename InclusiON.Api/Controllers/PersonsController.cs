using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para la gestion de personas con discapacidad.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PersonsController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public PersonsController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        #region Queries

        /// <summary>
        /// Obtiene una lista paginada de personas con discapacidad.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "persons:read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<PersonListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<PersonListItemResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<PersonListItemResponse>>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResponse<PersonListItemResponse>>>> GetPersons(
            [FromQuery] GetPersonsRequest request,
            [FromServices] IQueryHandler<GetPersonsQuery, ApiResponse<PagedResponse<PersonListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            request.Validate();

            var query = new GetPersonsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.DisabilityTypeId,
                request.AutonomyLevelId,
                request.IsActive,
                request.SortBy,
                request.SortDirection);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una persona por su ID.
        /// </summary>
        [HttpGet("{personId:guid}")]
        [Authorize(Policy = "persons:read")]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponse>>> GetPersonById(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonByIdQuery, ApiResponse<PersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPersonByIdQuery(personId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Crea una nueva persona con discapacidad.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "persons:create")]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponse>>> CreatePerson(
            [FromBody] CreatePersonRequest request,
            [FromServices] ICommandHandler<CreatePersonCommand, ApiResponse<PersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<PersonResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new CreatePersonCommand(
                request.FirstName,
                request.LastName,
                request.DocumentNumber,
                request.BirthDate,
                request.DisabilityTypeId,
                request.PhotoUrl,
                request.AttentionLevel,
                request.CommunicationLevel,
                request.UsesAAC,
                request.UsesSignLanguage,
                request.MotorSkillLevel,
                request.InterestsAndMotivators,
                request.LearningStyle,
                request.AvailableResources,
                request.AdditionalTherapies,
                request.RequiresLargeFont,
                request.RequiresHighContrast,
                request.VisualNoiseSensitivity,
                request.SoundSensitivity,
                request.AutonomyLevelId,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId,
                request.AvatarColor);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                return result.ToActionResult();
            }

            return CreatedAtAction(
                nameof(GetPersonById),
                new { personId = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        [HttpPut("{personId:guid}")]
        [Authorize(Policy = "persons:update")]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponse>>> UpdatePerson(
            Guid personId,
            [FromBody] UpdatePersonRequest request,
            [FromServices] ICommandHandler<UpdatePersonCommand, ApiResponse<PersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<PersonResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new UpdatePersonCommand(
                personId,
                request.FirstName,
                request.LastName,
                request.DocumentNumber,
                request.BirthDate,
                request.DisabilityTypeId,
                request.PhotoUrl,
                request.AttentionLevel,
                request.CommunicationLevel,
                request.UsesAAC,
                request.UsesSignLanguage,
                request.MotorSkillLevel,
                request.InterestsAndMotivators,
                request.LearningStyle,
                request.AvailableResources,
                request.AdditionalTherapies,
                request.RequiresLargeFont,
                request.RequiresHighContrast,
                request.VisualNoiseSensitivity,
                request.SoundSensitivity,
                request.AutonomyLevelId,
                request.SupervisorUserId,
                request.AvatarColor);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Login Method

        /// <summary>
        /// Actualiza el metodo de login de una persona con discapacidad.
        /// Solo el propio usuario o un supervisor autorizado puede realizar esta accion.
        /// </summary>
        [HttpPut("{userId:guid}/login-method")]
        [Authorize(Policy = "persons:update")]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UpdateLoginMethodResponse>>> UpdateLoginMethod(
            Guid userId,
            [FromBody] UpdateLoginMethodRequest request,
            [FromServices] ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UpdateLoginMethodResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new UpdateLoginMethodCommand(
                userId,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                if (result.Message.Contains("no encontrad", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Actualiza el metodo de login del usuario autenticado (persona con discapacidad).
        /// </summary>
        [HttpPut("me/login-method")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<UpdateLoginMethodResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UpdateLoginMethodResponse>>> UpdateMyLoginMethod(
            [FromBody] UpdateLoginMethodRequest request,
            [FromServices] ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<UpdateLoginMethodResponse>.ErrorResult(ErrorMessages.TokenInvalid));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UpdateLoginMethodResponse>.ErrorResult(ErrorMessages.ValidationFailed, errors));
            }

            var command = new UpdateLoginMethodCommand(
                userId.Value,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
            {
                if (result.Message.Contains("no encontrad", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        #endregion

    }
}
