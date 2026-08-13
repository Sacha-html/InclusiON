using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using InclusiON.Api.Extensions;
using InclusiON.Api.Filters;
using InclusiON.Application.Authorization;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
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
        private readonly IResourceAuthorizationService _resourceAuthz;
        private readonly IPersonsRepository _personsRepository;

        public PersonsController(
            IHttpContextService httpContextService,
            IResourceAuthorizationService resourceAuthz,
            IPersonsRepository personsRepository)
        {
            _httpContextService = httpContextService;
            _resourceAuthz      = resourceAuthz;
            _personsRepository  = personsRepository;
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

            var accessibleIds = _httpContextService.IsGlobalAdmin()
                ? null
                : await _resourceAuthz.GetAccessiblePersonIdsAsync(cancellationToken);

            var query = new GetPersonsQuery(
                request.Page, request.PageSize, request.Search,
                request.DisabilityTypeId, request.AutonomyLevelId, request.IsActive,
                request.SortBy, request.SortDirection, request.InstitutionIds,
                request.RepresentativeSearch, accessibleIds);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una persona por su ID.
        /// </summary>
        [HttpGet("{personId}")]
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
            var result = await handler.HandleAsync(new GetPersonByIdQuery(personId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene los profesionales asignados a una persona con discapacidad.
        /// </summary>
        [HttpGet("{personId}/professionals")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonProfessionalResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonProfessionalResponse>>>> GetPersonProfessionals(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonProfessionalsQuery, ApiResponse<List<PersonProfessionalResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonProfessionalsQuery(personId), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los familiares vinculados a una persona.
        /// </summary>
        [HttpGet("{personId}/representatives")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonRepresentativeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonRepresentativeResponse>>>> GetPersonRepresentatives(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonRepresentativesQuery, ApiResponse<List<PersonRepresentativeResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonRepresentativesQuery(personId), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el historial de vinculaciones de una persona.
        /// </summary>
        [HttpGet("{personId}/link-history")]
        [OutputCache(PolicyName = "history")]
        [Authorize(Policy = "family:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonRepresentativeHistoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonRepresentativeHistoryResponse>>>> GetPersonLinkHistory(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonLinkHistoryQuery, ApiResponse<List<PersonRepresentativeHistoryResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonLinkHistoryQuery(personId), cancellationToken);
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
                request.FirstName, request.LastName, request.DocumentNumber,
                request.BirthDate, request.DisabilityTypeId, request.PhotoUrl,
                request.AttentionLevel, request.CommunicationLevel,
                request.UsesAAC, request.UsesSignLanguage, request.MotorSkillLevel,
                request.InterestsAndMotivators, request.LearningStyle,
                request.AvailableResources, request.AdditionalTherapies,
                request.RequiresLargeFont, request.RequiresHighContrast,
                request.VisualNoiseSensitivity, request.SoundSensitivity,
                request.ColorBlindnessType, request.AutonomyLevelId,
                request.LoginMethodId, request.Pin, request.SupervisorUserId,
                request.AvatarColor);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetPersonById), new { personId = result.Data!.Id }, result);
        }

        /// <summary>
        /// Crea una nueva persona con discapacidad vinculada a su tutor en una sola transacción.
        /// </summary>
        [HttpPost("with-tutor")]
        [Authorize(Policy = "persons:create")]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<PersonResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponse>>> CreatePersonWithTutor(
            [FromBody] CreatePersonWithTutorRequest request,
            [FromServices] ICommandHandler<CreatePersonWithTutorCommand, ApiResponse<PersonResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var command = new CreatePersonWithTutorCommand(
                request.Student.FirstName, request.Student.LastName, request.Student.DocumentNumber,
                request.Student.BirthDate, request.Student.DisabilityTypeId, request.Student.PhotoUrl,
                request.Student.AttentionLevel, request.Student.CommunicationLevel,
                request.Student.UsesAAC, request.Student.UsesSignLanguage, request.Student.MotorSkillLevel,
                request.Student.InterestsAndMotivators, request.Student.LearningStyle,
                request.Student.AvailableResources, request.Student.AdditionalTherapies,
                request.Student.RequiresLargeFont, request.Student.RequiresHighContrast,
                request.Student.VisualNoiseSensitivity, request.Student.SoundSensitivity,
                request.Student.ColorBlindnessType, request.Student.AutonomyLevelId,
                request.Student.LoginMethodId, request.Student.Pin, request.Student.AvatarColor,
                request.TutorFirstName, request.TutorLastName, request.TutorEmail,
                request.TutorDocumentNumber, request.TutorPhone, request.TutorRelationship,
                request.ClassroomId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetPersonById), new { personId = result.Data!.Id }, result);
        }

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        [HttpPut("{personId}")]
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
                personId, request.FirstName, request.LastName, request.DocumentNumber,
                request.BirthDate, request.DisabilityTypeId, request.PhotoUrl,
                request.AttentionLevel, request.CommunicationLevel,
                request.UsesAAC, request.UsesSignLanguage, request.MotorSkillLevel,
                request.InterestsAndMotivators, request.LearningStyle,
                request.AvailableResources, request.AdditionalTherapies,
                request.RequiresLargeFont, request.RequiresHighContrast,
                request.VisualNoiseSensitivity, request.SoundSensitivity,
                request.ColorBlindnessType, request.AutonomyLevelId,
                request.SupervisorUserId, request.AvatarColor);

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva una persona con discapacidad.
        /// </summary>
        [HttpPut("{personId}/deactivate")]
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
            var result = await handler.HandleAsync(new DeactivatePersonCommand(personId), cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Login Method

        /// <summary>
        /// Lista los candidatos a supervisor (profesionales asignados + familiares vinculados activos).
        /// </summary>
        [HttpGet("{personId}/supervisor-candidates")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<SupervisorCandidateResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<SupervisorCandidateResponse>>>> GetSupervisorCandidates(
            Guid personId,
            [FromServices] IQueryHandler<GetSupervisorCandidatesQuery, ApiResponse<PagedResponse<SupervisorCandidateResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetSupervisorCandidatesQuery(personId, page, pageSize), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Actualiza el metodo de login de una persona con discapacidad.
        /// </summary>
        [HttpPut("{userId}/login-method")]
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
            var person = await _personsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (person is null)
                return ApiResponse<UpdateLoginMethodResponse>.NotFound("Persona").ToActionResult();

            if (!await _resourceAuthz.CanAccessPersonAsync(person.Id, AccessMode.Write, cancellationToken))
                return ApiResponse<UpdateLoginMethodResponse>.Forbidden().ToActionResult();

            var command = new UpdateLoginMethodCommand(userId, request.LoginMethodId, request.Pin, request.SupervisorUserId);
            var result  = await handler.HandleAsync(command, cancellationToken);
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
            if (userId is null)
                return Unauthorized(ApiResponse<UpdateLoginMethodResponse>.ErrorResult(ErrorMessages.TokenInvalid));

            var command = new UpdateLoginMethodCommand(userId.Value, request.LoginMethodId, request.Pin, request.SupervisorUserId);
            var result  = await handler.HandleAsync(command, cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Skill Profile

        /// <summary>
        /// Obtiene el perfil de habilidades (areas asignadas) de una persona.
        /// </summary>
        [HttpGet("{personId}/skill-profile")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonSkillProfileResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonSkillProfileResponse>>>> GetSkillProfile(
            Guid personId,
            [FromQuery] bool all,
            [FromServices] IQueryHandler<GetPersonSkillProfileQuery, ApiResponse<List<PersonSkillProfileResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonSkillProfileQuery(personId, all), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene actividades recomendadas para una persona (ordenadas por compatibilidad).
        /// </summary>
        [HttpGet("{personId}/recommended-activities")]
        [Authorize(Policy = "persons:read")]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> GetRecommendedActivities(
            Guid personId,
            [FromServices] IQueryHandler<GetRecommendedActivitiesQuery, ApiResponse<List<ActivityListItemResponse>>> handler,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<ActivityListItemResponse>>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new GetRecommendedActivitiesQuery(personId, professionalId.Value, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Asigna un area de habilidad a una persona.
        /// </summary>
        [HttpPost("{personId}/skill-profile")]
        [Authorize(Policy = "persons:update")]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<PersonSkillProfileResponse>>> AddSkillArea(
            Guid personId,
            [FromBody] AddSkillAreaRequest request,
            [FromServices] ICommandHandler<AddSkillAreaCommand, ApiResponse<PersonSkillProfileResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new AddSkillAreaCommand(personId, request.SkillAreaId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Desactiva un area de habilidad de una persona (no la elimina).
        /// </summary>
        [HttpPut("{personId}/skill-profile/{areaId:int}")]
        [Authorize(Policy = "persons:update")]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonSkillProfileResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonSkillProfileResponse>>> DeactivateSkillArea(
            Guid personId,
            int areaId,
            [FromServices] ICommandHandler<DeactivateSkillAreaCommand, ApiResponse<PersonSkillProfileResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new DeactivateSkillAreaCommand(personId, areaId), cancellationToken);
            return result.ToActionResult();
        }

        #endregion

        #region Help Request

        /// <summary>
        /// Obtiene la configuración de accesibilidad de una persona.
        /// </summary>
        [HttpGet("{personId}/accessibility")]
        [Authorize(Policy = Permissions.Persons.Read)]
        [PersonAccess(AccessMode.Read)]
        [ProducesResponseType(typeof(ApiResponse<PersonAccessibilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonAccessibilityResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonAccessibilityResponse>>> GetAccessibility(
            Guid personId,
            [FromServices] IQueryHandler<GetPersonAccessibilityQuery, ApiResponse<PersonAccessibilityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(new GetPersonAccessibilityQuery(personId), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza la configuración de accesibilidad de una persona.
        /// </summary>
        [HttpPut("{personId}/accessibility")]
        [Authorize(Policy = Permissions.Persons.Update)]
        [PersonAccess(AccessMode.Write)]
        [ProducesResponseType(typeof(ApiResponse<PersonAccessibilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonAccessibilityResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonAccessibilityResponse>>> UpdateAccessibility(
            Guid personId,
            [FromBody] UpdatePersonAccessibilityRequest request,
            [FromServices] ICommandHandler<UpdatePersonAccessibilityCommand, ApiResponse<PersonAccessibilityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var result = await handler.HandleAsync(
                new UpdatePersonAccessibilityCommand(
                    personId,
                    request.RequiresLargeFont,
                    request.RequiresHighContrast,
                    request.VisualNoiseSensitivity,
                    request.SoundSensitivity,
                    request.ColorBlindnessType),
                cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Solicitud de ayuda urgente desde el portal AAC.
        /// Notifica vía SignalR a todos los profesionales supervisores activos de la persona.
        /// </summary>
        [HttpPost("me/help-request")]
        [Authorize(Policy = Permissions.Activities.Respond)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RequestHelp(
            [FromServices] ICommandHandler<RequestHelpCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var personId = _httpContextService.GetCurrentEntityId();
            if (personId is null)
                return NotFound(ApiResponse<object>.NotFound("Persona"));

            var result = await handler.HandleAsync(new RequestHelpCommand(personId.Value), cancellationToken);
            return Ok(result);
        }

        #endregion
    }
}
