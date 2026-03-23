using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Api.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Data;
using InclusiON.Domain.Models;
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
        private readonly AppDbContext _context;

        public PersonsController(
            IHttpContextService httpContextService,
            AppDbContext context)
        {
            _httpContextService = httpContextService;
            _context = context;
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
                request.SortDirection,
                request.InstitutionId);

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

        #region Skill Profile

        /// <summary>
        /// Obtiene el perfil de habilidades (areas asignadas) de una persona.
        /// </summary>
        [HttpGet("{personId:guid}/skill-profile")]
        [Authorize(Policy = "persons:read")]
        [ProducesResponseType(typeof(ApiResponse<List<PersonSkillProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonSkillProfileResponse>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<PersonSkillProfileResponse>>>> GetSkillProfile(
            Guid personId,
            [FromQuery] bool all = false,
            CancellationToken cancellationToken = default)
        {
            var personExists = await _context.PersonsWithDisability
                .AnyAsync(p => p.Id == personId, cancellationToken);

            if (!personExists)
            {
                return NotFound(ApiResponse<List<PersonSkillProfileResponse>>.NotFound("Persona"));
            }

            var query = _context.PersonSkillProfiles
                .Where(psp => psp.PersonId == personId);

            if (!all)
            {
                query = query.Where(psp => psp.IsActive);
            }

            var profiles = await query
                .Include(psp => psp.SkillArea)
                .OrderBy(psp => psp.SkillArea.DisplayOrder)
                .Select(psp => new PersonSkillProfileResponse
                {
                    SkillAreaId = psp.SkillAreaId,
                    SkillAreaName = psp.SkillArea.Name,
                    Color = psp.SkillArea.Color,
                    Icon = psp.SkillArea.Icon,
                    IsActive = psp.IsActive,
                    AssignedAt = psp.AssignedAt
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<PersonSkillProfileResponse>>.SuccessResult(profiles));
        }

        /// <summary>
        /// Asigna un area de habilidad a una persona.
        /// </summary>
        [HttpPost("{personId:guid}/skill-profile")]
        [Authorize(Policy = "persons:update")]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<PersonSkillProfileResponse>>> AddSkillArea(
            Guid personId,
            [FromBody] AddSkillAreaRequest request,
            CancellationToken cancellationToken = default)
        {
            var personExists = await _context.PersonsWithDisability
                .AnyAsync(p => p.Id == personId, cancellationToken);

            if (!personExists)
            {
                return NotFound(ApiResponse<PersonSkillProfileResponse>.NotFound("Persona"));
            }

            var skillArea = await _context.SkillAreas
                .FirstOrDefaultAsync(sa => sa.Id == request.SkillAreaId, cancellationToken);

            if (skillArea == null)
            {
                return NotFound(ApiResponse<PersonSkillProfileResponse>.NotFound("Area de habilidad"));
            }

            var existing = await _context.PersonSkillProfiles
                .FirstOrDefaultAsync(psp => psp.PersonId == personId && psp.SkillAreaId == request.SkillAreaId, cancellationToken);

            if (existing != null)
            {
                if (existing.IsActive)
                {
                    return Conflict(ApiResponse<PersonSkillProfileResponse>.Conflict(
                        ErrorCode.Conflict,
                        "El area de habilidad ya esta asignada y activa para esta persona."));
                }

                // Reactivar
                existing.IsActive = true;
                existing.AssignedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                var reactivatedResponse = new PersonSkillProfileResponse
                {
                    SkillAreaId = existing.SkillAreaId,
                    SkillAreaName = skillArea.Name,
                    Color = skillArea.Color,
                    Icon = skillArea.Icon,
                    IsActive = existing.IsActive,
                    AssignedAt = existing.AssignedAt
                };

                return Ok(ApiResponse<PersonSkillProfileResponse>.SuccessResult(reactivatedResponse, "Area de habilidad reactivada exitosamente."));
            }

            var profile = new PersonSkillProfile
            {
                PersonId = personId,
                SkillAreaId = request.SkillAreaId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.PersonSkillProfiles.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new PersonSkillProfileResponse
            {
                SkillAreaId = profile.SkillAreaId,
                SkillAreaName = skillArea.Name,
                Color = skillArea.Color,
                Icon = skillArea.Icon,
                IsActive = profile.IsActive,
                AssignedAt = profile.AssignedAt
            };

            return Ok(ApiResponse<PersonSkillProfileResponse>.SuccessResult(response, "Area de habilidad asignada exitosamente."));
        }

        /// <summary>
        /// Desactiva un area de habilidad de una persona (no la elimina).
        /// </summary>
        [HttpPut("{personId:guid}/skill-profile/{areaId:int}")]
        [Authorize(Policy = "persons:update")]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonSkillProfileResponse>>> DeactivateSkillArea(
            Guid personId,
            int areaId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.PersonSkillProfiles
                .Include(psp => psp.SkillArea)
                .FirstOrDefaultAsync(psp => psp.PersonId == personId && psp.SkillAreaId == areaId, cancellationToken);

            if (profile == null)
            {
                return NotFound(ApiResponse<PersonSkillProfileResponse>.NotFound("Perfil de habilidad"));
            }

            profile.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);

            var response = new PersonSkillProfileResponse
            {
                SkillAreaId = profile.SkillAreaId,
                SkillAreaName = profile.SkillArea.Name,
                Color = profile.SkillArea.Color,
                Icon = profile.SkillArea.Icon,
                IsActive = profile.IsActive,
                AssignedAt = profile.AssignedAt
            };

            return Ok(ApiResponse<PersonSkillProfileResponse>.SuccessResult(response, "Area de habilidad desactivada exitosamente."));
        }

        #endregion

    }
}
