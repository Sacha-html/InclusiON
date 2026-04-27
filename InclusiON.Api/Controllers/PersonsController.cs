using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Responses;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Responses.Family;
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
        private readonly IResourceAuthorizationService _resourceAuthz;

        public PersonsController(
            IHttpContextService httpContextService,
            AppDbContext context,
            IResourceAuthorizationService resourceAuthz)
        {
            _httpContextService = httpContextService;
            _context = context;
            _resourceAuthz = resourceAuthz;
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

            // HU-IN-172: scoping por rol. GlobalAdmin ve todo; los demás sólo sus personas asignadas.
            var accessibleIds = _httpContextService.IsGlobalAdmin()
                ? null
                : await _resourceAuthz.GetAccessiblePersonIdsAsync(cancellationToken);

            var query = new GetPersonsQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.DisabilityTypeId,
                request.AutonomyLevelId,
                request.IsActive,
                request.SortBy,
                request.SortDirection,
                request.InstitutionIds,
                request.RepresentativeSearch,
                accessibleIds);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una persona por su ID.
        /// </summary>
        [HttpGet("{personId:guid}")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
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

        /// <summary>
        /// Obtiene los profesionales asignados a una persona con discapacidad.
        /// </summary>
        [HttpGet("{personId:guid}/professionals")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonProfessionalResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonProfessionalResponse>>>> GetPersonProfessionals(
            Guid personId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await _context.ProfessionalPersons
                .Include(pp => pp.Professional)
                .Where(pp => pp.PersonId == personId && pp.IsActive)
                .OrderByDescending(pp => pp.IsPrimaryProfessional)
                .ThenByDescending(pp => pp.AssignedAt)
                .ToListAsync(cancellationToken);

            var response = assignments.Select(pp => new PersonProfessionalResponse
            {
                ProfessionalId = pp.ProfessionalId,
                PersonId = pp.PersonId,
                PersonFirstName = pp.Professional.FirstName,
                PersonLastName = pp.Professional.LastName,
                PersonFullName = $"{pp.Professional.FirstName} {pp.Professional.LastName}",
                IsPrimaryProfessional = pp.IsPrimaryProfessional,
                CanSuperviseLogin = pp.CanSuperviseLogin,
                IsActive = pp.IsActive,
                AssignedAt = pp.AssignedAt
            }).ToList();

            return Ok(ApiResponse<List<PersonProfessionalResponse>>.SuccessResult(response));
        }

        /// <summary>
        /// Obtiene los familiares vinculados a una persona.
        /// </summary>
        [HttpGet("{personId:guid}/representatives")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonRepresentativeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonRepresentativeResponse>>>> GetPersonRepresentatives(
            Guid personId,
            CancellationToken cancellationToken = default)
        {
            var representatives = await _context.PersonRepresentatives
                .Include(pr => pr.Representative)
                    .ThenInclude(r => r.User)
                .Where(pr => pr.PersonId == personId)
                .OrderByDescending(pr => pr.IsPrimary)
                .ThenBy(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = representatives.Select(pr => new PersonRepresentativeResponse
            {
                PersonId = pr.PersonId,
                RepresentativeId = pr.RepresentativeId,
                RepresentativeFullName = $"{pr.Representative.FirstName} {pr.Representative.LastName}",
                Relationship = pr.Relationship,
                IsPrimary = pr.IsPrimary,
                IsActive = pr.IsActive,
                CreatedAt = pr.CreatedAt,
                UpdatedAt = pr.UpdatedAt,
                EndedAt = pr.EndedAt,
                UnlinkObservation = pr.UnlinkObservation
            }).ToList();

            return Ok(ApiResponse<List<PersonRepresentativeResponse>>.SuccessResult(response));
        }

        /// <summary>
        /// Obtiene el historial de vinculaciones de una persona.
        /// </summary>
        [HttpGet("{personId:guid}/link-history")]
        [Authorize(Policy = "family:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonRepresentativeHistoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonRepresentativeHistoryResponse>>>> GetPersonLinkHistory(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonLinkHistoryQuery, ApiResponse<List<PersonRepresentativeHistoryResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPersonLinkHistoryQuery(personId);
            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
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
                request.ColorBlindnessType,
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
        [PersonAccess(AccessMode.Write)]
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
                request.ColorBlindnessType,
                request.AutonomyLevelId,
                request.SupervisorUserId,
                request.AvatarColor);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva una persona con discapacidad.
        /// </summary>
        [HttpPut("{personId:guid}/deactivate")]
        [Authorize(Policy = "persons:delete")]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponse>>> DeactivatePerson(
            Guid personId,
            [FromServices] ICommandHandler<DeactivatePersonCommand, ApiResponse<PersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new DeactivatePersonCommand(personId);
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Login Method

        /// <summary>
        /// Lista los candidatos a supervisor (profesionales asignados + familiares vinculados activos).
        /// Usado en el form de cambio de metodo de login cuando se elige ASSISTED.
        /// </summary>
        [HttpGet("{personId:guid}/supervisor-candidates")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<SupervisorCandidateResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<SupervisorCandidateResponse>>>> GetSupervisorCandidates(
            Guid personId,
            [FromServices] IQueryHandler<GetSupervisorCandidatesQuery, ApiResponse<List<SupervisorCandidateResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetSupervisorCandidatesQuery(personId), cancellationToken);
            return Ok(result);
        }

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

            var command = new UpdateLoginMethodCommand(
                userId,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
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

            var command = new UpdateLoginMethodCommand(
                userId.Value,
                request.LoginMethodId,
                request.Pin,
                request.SupervisorUserId);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Skill Profile

        /// <summary>
        /// Obtiene el perfil de habilidades (areas asignadas) de una persona.
        /// </summary>
        [HttpGet("{personId:guid}/skill-profile")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonSkillProfileResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonSkillProfileResponse>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<PersonSkillProfileResponse>>>> GetSkillProfile(
            Guid personId,
            [FromQuery] bool all = false,
            CancellationToken cancellationToken = default)
        {
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
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<PersonSkillProfileResponse>>> AddSkillArea(
            Guid personId,
            [FromBody] AddSkillAreaRequest request,
            CancellationToken cancellationToken = default)
        {
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
        [PersonAccess(AccessMode.Write)]
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
