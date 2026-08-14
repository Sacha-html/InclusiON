using InclusiON.Api.Extensions;
using InclusiON.Api.ModelBinders;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Activities;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using InclusiON.DTOs.Responses.Persons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public ActivitiesController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        /// <summary>Búsqueda semántica de actividades por texto libre.</summary>
        [HttpGet("search")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> SearchActivitiesSemantic(
            [FromQuery] string text,
            [FromServices] IQueryHandler<SearchActivitiesSemanticQuery, ApiResponse<List<ActivityListItemResponse>>> handler,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest(ApiResponse<List<ActivityListItemResponse>>.ErrorResult(
                    ErrorCode.InvalidInput, "El texto de búsqueda es requerido."));

            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<ActivityListItemResponse>>.NotFound("Profesional"));

            limit = Math.Clamp(limit, 1, 50);

            var result = await handler.HandleAsync(
                new SearchActivitiesSemanticQuery(professionalId.Value, text, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Obtiene actividades similares a una actividad existente.</summary>
        [HttpGet("{id}/similar")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> GetSimilarActivities(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetSimilarActivitiesQuery, ApiResponse<List<ActivityListItemResponse>>> handler,
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<ActivityListItemResponse>>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new GetSimilarActivitiesQuery(professionalId.Value, id, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Obtiene personas compatibles con una actividad (ordenadas por compatibilidad).</summary>
        [HttpGet("{id}/compatible-persons")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PersonListItemResponse>>>> GetCompatiblePersons(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetCompatiblePersonsQuery, ApiResponse<List<PersonListItemResponse>>> handler,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<List<PersonListItemResponse>>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new GetCompatiblePersonsQuery(id, professionalId.Value, limit),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>Lista paginada de actividades (propias + estándar).</summary>
        [HttpGet]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ActivityListItemResponse>>>> GetActivities(
            [FromQuery] GetActivitiesRequest request,
            [FromServices] IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<PagedResponse<ActivityListItemResponse>>.NotFound("Profesional"));

            request.Validate();

            var query = new GetActivitiesQuery(
                professionalId.Value,
                request.Search,
                request.CategoryId,
                request.SkillAreaId,
                request.TemplateTypeId,
                request.IsActive,
                request.IsStandard,
                request.IsTemplate,
                request.Page,
                request.PageSize);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>Detalle de una actividad por ID.</summary>
        [HttpGet("{id}")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> GetActivity(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromServices] IQueryHandler<GetActivityByIdQuery, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var entityId = _httpContextService.GetCurrentEntityId() ?? Guid.Empty;

            var result = await handler.HandleAsync(new GetActivityByIdQuery(id, entityId), cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Crea una nueva actividad.</summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Activities.Create)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> CreateActivity(
            [FromBody] CreateActivityRequest request,
            [FromServices] ICommandHandler<CreateActivityCommand, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var command = new CreateActivityCommand(
                professionalId.Value,
                request.Title,
                request.Description,
                request.Instructions,
                request.CategoryId,
                request.SkillAreaId,
                request.ComplexityLevel,
                request.EstimatedDurationMinutes,
                request.RequiresSupervision,
                request.HasVisualSupport,
                request.HasAudioSupport,
                request.UsesEasyReading,
                request.UsesPictograms,
                request.ResourcesUrl,
                request.TemplateTypeId,
                request.ContentJson,
                request.IsTemplate);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetActivity), new { id = result.Data!.Id }, result);
        }

        /// <summary>Actualiza una actividad existente.</summary>
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Activities.Update)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> UpdateActivity(
            string id,
            [FromBody] UpdateActivityRequest request,
            [FromServices] ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>> handler,
            [FromServices] IEncryptionService encryptionService,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var toDecrypt = id.StartsWith("ENC:", StringComparison.Ordinal)
                ? InclusiON.Api.Converters.EncryptedGuidConverter.ToStandardBase64(id)
                : id;
            var decrypted = encryptionService.Decrypt(toDecrypt);
            if (!int.TryParse(decrypted, out var decryptedId))
            {
                return BadRequest(ApiResponse<ActivityResponse>.ErrorResult(ErrorCode.InvalidInput, "Identificador de actividad inválido o con formato incorrecto."));
            }

            var command = new UpdateActivityCommand(
                decryptedId,
                professionalId.Value,
                request.Title,
                request.Description,
                request.Instructions,
                request.CategoryId,
                request.SkillAreaId,
                request.ComplexityLevel,
                request.EstimatedDurationMinutes,
                request.RequiresSupervision,
                request.HasVisualSupport,
                request.HasAudioSupport,
                request.UsesEasyReading,
                request.UsesPictograms,
                request.ResourcesUrl,
                request.ContentJson,
                request.IsTemplate);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Activa o da de baja una actividad (máquina de estados).</summary>
        [HttpPatch("{id}")]
        [Authorize(Policy = Permissions.Activities.Update)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> PatchActivityStatus(
            [ModelBinder(typeof(EncryptedIntModelBinder))] int id,
            [FromBody] PatchStatusRequest request,
            [FromServices] ICommandHandler<PatchActivityStatusCommand, ApiResponse<ActivityResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            var result = await handler.HandleAsync(
                new PatchActivityStatusCommand(id, professionalId.Value, request.IsActive),
                cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return Ok(result);
        }

        /// <summary>Obtiene la lista de plantillas globales de actividades.</summary>
        [HttpGet("templates")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<ActivityListItemResponse>>>> GetTemplates(
            [FromQuery] GetActivitiesRequest request,
            [FromServices] IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<PagedResponse<ActivityListItemResponse>>.NotFound("Profesional"));

            request.Validate();

            var query = new GetActivitiesQuery(
                professionalId.Value,
                request.Search,
                request.CategoryId,
                request.SkillAreaId,
                request.TemplateTypeId,
                request.IsActive,
                request.IsStandard,
                true, // IsTemplate = true
                request.Page,
                request.PageSize);

            var result = await handler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>Obtiene las plantillas oficiales del Roadmap secuencial ordenadas ascendentemente (Nivel 1 al 10).</summary>
        [HttpGet("roadmap")]
        [Authorize(Policy = Permissions.Activities.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ActivityListItemResponse>>>> GetRoadmap(
            [FromServices] InclusiON.Application.Interfaces.Repositories.IActivitiesRepository repository,
            [FromServices] IEncryptionService encryptionService,
            CancellationToken cancellationToken = default)
        {
            var activities = await repository.GetRoadmapTemplatesAsync(cancellationToken);
            var response = activities.Select(a =>
            {
                var item = ActivityListItemResponse.From(a);
                var encrypted = encryptionService.Encrypt(a.Id.ToString());
                item.EncryptedId = encrypted.Replace('+', '-').Replace('/', '_').TrimEnd('=');
                return item;
            }).ToList();

            return Ok(ApiResponse<List<ActivityListItemResponse>>.SuccessResult(response));
        }

        /// <summary>Clona una actividad plantilla para el profesional logueado.</summary>
        [HttpPost("{id}/clone")]
        [Authorize(Policy = Permissions.Activities.Create)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ActivityResponse>>> CloneActivity(
            string id,
            [FromServices] InclusiON.Application.Interfaces.Repositories.IActivitiesRepository repository,
            [FromServices] IUnitOfWork unitOfWork,
            [FromServices] IEncryptionService encryptionService,
            CancellationToken cancellationToken = default)
        {
            var professionalId = _httpContextService.GetCurrentEntityId();
            if (professionalId is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Profesional"));

            // Desencriptar el ID de la actividad
            var toDecrypt = id.StartsWith("ENC:", StringComparison.Ordinal)
                ? InclusiON.Api.Converters.EncryptedGuidConverter.ToStandardBase64(id)
                : id;
            var decrypted = encryptionService.Decrypt(toDecrypt);
            if (!int.TryParse(decrypted, out var decryptedId))
            {
                return BadRequest(ApiResponse<ActivityResponse>.ErrorResult(ErrorCode.InvalidInput, "Identificador de actividad inválido o con formato incorrecto."));
            }

            // Buscar la actividad original
            var original = await repository.GetByIdAsync(decryptedId, cancellationToken);
            if (original is null)
                return NotFound(ApiResponse<ActivityResponse>.NotFound("Actividad original"));

            // Clonar la actividad
            var now = DateTime.UtcNow;
            var clone = new InclusiON.Domain.Models.Activity
            {
                ProfessionalId = professionalId.Value,
                Title = $"{original.Title} (Copia)",
                Description = original.Description,
                Instructions = original.Instructions,
                CategoryId = original.CategoryId,
                SkillAreaId = original.SkillAreaId,
                ComplexityLevel = original.ComplexityLevel,
                EstimatedDurationMinutes = original.EstimatedDurationMinutes,
                RequiresSupervision = original.RequiresSupervision,
                HasVisualSupport = original.HasVisualSupport,
                HasAudioSupport = original.HasAudioSupport,
                UsesEasyReading = original.UsesEasyReading,
                UsesPictograms = original.UsesPictograms,
                ResourcesUrl = original.ResourcesUrl,
                IsStandardActivity = false,
                IsTemplate = false,
                IsActive = true,
                CreatedAt = now,
            };

            if (original.Content is not null)
            {
                clone.Content = new InclusiON.Domain.Models.ActivityContent
                {
                    TemplateTypeId = original.Content.TemplateTypeId,
                    ContentJson = original.Content.ContentJson,
                    CreatedAt = now,
                };
            }

            clone.Embedding = new InclusiON.Domain.Models.ActivityEmbedding
            {
                Model = "paraphrase-multilingual-MiniLM-L12-v2",
                Dimensions = 384,
                CreatedAt = now,
            };

            await repository.CreateAsync(clone, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var created = await repository.GetByIdAsync(clone.Id, cancellationToken);
            var dto = ActivityResponse.From(created!);
            dto.EncryptedId = ToUrlSafeBase64(encryptionService.Encrypt(created!.Id.ToString()));

            return CreatedAtAction(nameof(GetActivity), new { id = dto.EncryptedId }, ApiResponse<ActivityResponse>.SuccessResult(dto, "Plantilla clonada y agregada a tus actividades."));
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
