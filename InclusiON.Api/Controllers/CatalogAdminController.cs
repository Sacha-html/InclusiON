using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Catalogs;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador de administracion de catalogos del sistema.
    /// Provee endpoints de escritura (crear/actualizar) para entidades de catalogo.
    /// </summary>
    [Route("api/admin/catalogs")]
    [ApiController]
    [Authorize(Policy = "settings:update")]
    [Authorize(Policy = "global-admin")]
    [Produces("application/json")]
    public class CatalogAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogAdminController(AppDbContext context)
        {
            _context = context;
        }

        #region Disability Types

        /// <summary>
        /// Crea un nuevo tipo de discapacidad.
        /// </summary>
        [HttpPost("disability-types")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateDisabilityType(
            [FromBody] CreateDisabilityTypeRequest request,
            CancellationToken cancellationToken)
        {
            return CreateCatalogAsync<DisabilityType, CatalogItemResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new DisabilityType
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de discapacidad",
                cancellationToken);
        }

        /// <summary>
        /// Actualiza un tipo de discapacidad existente.
        /// </summary>
        [HttpPut("disability-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateDisabilityType(
            int id, [FromBody] UpdateDisabilityTypeRequest request, CancellationToken cancellationToken)
        {
            return UpdateCatalogAsync<DisabilityType, CatalogItemResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.IsActive = request.IsActive;
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de discapacidad",
                cancellationToken);
        }

        #endregion

        #region Autonomy Levels

        /// <summary>
        /// Crea un nuevo nivel de autonomia.
        /// </summary>
        [HttpPost("autonomy-levels")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> CreateAutonomyLevel(
            [FromBody] CreateAutonomyLevelRequest request, CancellationToken cancellationToken)
        {
            return CreateCatalogAsync<AutonomyLevel, AutonomyLevelResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new AutonomyLevel
                {
                    Name = request.Name,
                    Description = request.Description,
                    RequiresSupervision = request.RequiresSupervision,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = true
                },
                toResponse: e => new AutonomyLevelResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    RequiresSupervision = e.RequiresSupervision, DisplayOrder = e.DisplayOrder
                },
                "Nivel de autonomia",
                cancellationToken);
        }

        /// <summary>
        /// Actualiza un nivel de autonomia existente.
        /// </summary>
        [HttpPut("autonomy-levels/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> UpdateAutonomyLevel(
            int id, [FromBody] UpdateAutonomyLevelRequest request, CancellationToken cancellationToken)
        {
            return UpdateCatalogAsync<AutonomyLevel, AutonomyLevelResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.RequiresSupervision = request.RequiresSupervision;
                    e.DisplayOrder = request.DisplayOrder;
                    e.IsActive = request.IsActive;
                },
                toResponse: e => new AutonomyLevelResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    RequiresSupervision = e.RequiresSupervision, DisplayOrder = e.DisplayOrder
                },
                "Nivel de autonomia",
                cancellationToken);
        }

        #endregion

        #region Activity Categories

        /// <summary>
        /// Crea una nueva categoria de actividad.
        /// </summary>
        [HttpPost("activity-categories")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateActivityCategory(
            [FromBody] CreateActivityCategoryRequest request, CancellationToken cancellationToken)
        {
            return CreateCatalogAsync<ActivityCategory, CatalogItemResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new ActivityCategory
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = true
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Categoria de actividad",
                cancellationToken);
        }

        /// <summary>
        /// Actualiza una categoria de actividad existente.
        /// </summary>
        [HttpPut("activity-categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateActivityCategory(
            int id, [FromBody] UpdateActivityCategoryRequest request, CancellationToken cancellationToken)
        {
            return UpdateCatalogAsync<ActivityCategory, CatalogItemResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.IsActive = request.IsActive;
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Categoria de actividad",
                cancellationToken);
        }

        #endregion

        #region Skill Areas

        /// <summary>
        /// Crea una nueva area de habilidad.
        /// </summary>
        [HttpPost("skill-areas")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<SkillAreaResponse>>> CreateSkillArea(
            [FromBody] CreateSkillAreaRequest request, CancellationToken cancellationToken)
        {
            return CreateCatalogAsync<SkillArea, SkillAreaResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new SkillArea
                {
                    Name = request.Name,
                    Description = request.Description,
                    Icon = request.Icon,
                    Color = request.Color,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                toResponse: e => new SkillAreaResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    Icon = e.Icon, Color = e.Color, DisplayOrder = e.DisplayOrder
                },
                "Area de habilidad",
                cancellationToken);
        }

        /// <summary>
        /// Actualiza un area de habilidad existente.
        /// </summary>
        [HttpPut("skill-areas/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<SkillAreaResponse>>> UpdateSkillArea(
            int id, [FromBody] UpdateSkillAreaRequest request, CancellationToken cancellationToken)
        {
            return UpdateCatalogAsync<SkillArea, SkillAreaResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.Icon = request.Icon;
                    e.Color = request.Color;
                    e.DisplayOrder = request.DisplayOrder;
                    e.IsActive = request.IsActive;
                    e.UpdatedAt = DateTime.UtcNow;
                },
                toResponse: e => new SkillAreaResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    Icon = e.Icon, Color = e.Color, DisplayOrder = e.DisplayOrder
                },
                "Area de habilidad",
                cancellationToken);
        }

        #endregion

        #region Activity Template Types

        /// <summary>
        /// Crea un nuevo tipo de plantilla de actividad.
        /// </summary>
        [HttpPost("activity-template-types")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> CreateActivityTemplateType(
            [FromBody] CreateActivityTemplateTypeRequest request, CancellationToken cancellationToken)
        {
            var skillAreaExists = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Id == request.SkillAreaId, cancellationToken);

            if (!skillAreaExists)
                return NotFound(ApiResponse<ActivityTemplateTypeResponse>.NotFound("Area de habilidad"));

            return await CreateCatalogAsync<ActivityTemplateType, ActivityTemplateTypeResponse>(
                duplicateCheck: x => x.Name == request.Name || x.Code == request.Code,
                createEntity: () => new ActivityTemplateType
                {
                    SkillAreaId = request.SkillAreaId,
                    Name = request.Name,
                    Code = request.Code,
                    Description = request.Description,
                    ContentSchema = request.ContentSchema,
                    ComponentName = request.ComponentName,
                    UsesPictograms = request.UsesPictograms,
                    HasAudio = request.HasAudio,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                toResponse: MapActivityTemplateType,
                "Tipo de plantilla de actividad",
                cancellationToken);
        }

        /// <summary>
        /// Actualiza un tipo de plantilla de actividad existente.
        /// </summary>
        [HttpPut("activity-template-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> UpdateActivityTemplateType(
            int id, [FromBody] UpdateActivityTemplateTypeRequest request, CancellationToken cancellationToken)
        {
            var skillAreaExists = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Id == request.SkillAreaId, cancellationToken);

            if (!skillAreaExists)
                return NotFound(ApiResponse<ActivityTemplateTypeResponse>.NotFound("Area de habilidad"));

            return await UpdateCatalogAsync<ActivityTemplateType, ActivityTemplateTypeResponse>(
                id,
                duplicateCheck: x => (x.Name == request.Name || x.Code == request.Code) && x.Id != id,
                updateEntity: e =>
                {
                    e.SkillAreaId = request.SkillAreaId;
                    e.Name = request.Name;
                    e.Code = request.Code;
                    e.Description = request.Description;
                    e.ContentSchema = request.ContentSchema;
                    e.ComponentName = request.ComponentName;
                    e.UsesPictograms = request.UsesPictograms;
                    e.HasAudio = request.HasAudio;
                    e.DisplayOrder = request.DisplayOrder;
                    e.IsActive = request.IsActive;
                    e.UpdatedAt = DateTime.UtcNow;
                },
                toResponse: MapActivityTemplateType,
                "Tipo de plantilla de actividad",
                cancellationToken);
        }

        #endregion

        #region Login Methods

        /// <summary>
        /// Actualiza un metodo de login existente.
        /// </summary>
        [HttpPut("login-methods/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status409Conflict)]
        public Task<ActionResult<ApiResponse<LoginMethodResponse>>> UpdateLoginMethod(
            int id, [FromBody] UpdateLoginMethodCatalogRequest request, CancellationToken cancellationToken)
        {
            return UpdateCatalogAsync<LoginMethod, LoginMethodResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.DisplayOrder = request.DisplayOrder;
                    e.IsActive = request.IsActive;
                },
                toResponse: e => new LoginMethodResponse
                {
                    Id = e.Id, Code = e.Code, Name = e.Name, Description = e.Description,
                    RequiresPassword = e.RequiresPassword, RequiresPin = e.RequiresPin,
                    RequiresSupervisor = e.RequiresSupervisor, DisplayOrder = e.DisplayOrder
                },
                "Metodo de login",
                cancellationToken);
        }

        #endregion

        #region Generic Helpers

        private async Task<ActionResult<ApiResponse<TResponse>>> CreateCatalogAsync<TEntity, TResponse>(
            Expression<Func<TEntity, bool>> duplicateCheck,
            Func<TEntity> createEntity,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken)
            where TEntity : class
            where TResponse : class
        {
            var exists = await _context.Set<TEntity>().AnyAsync(duplicateCheck, cancellationToken);
            if (exists)
            {
                return Conflict(ApiResponse<TResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    $"Ya existe un(a) {entityDisplayName.ToLower()} con ese nombre"));
            }

            var entity = createEntity();
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<TResponse>.SuccessResult(toResponse(entity), $"{entityDisplayName} creado(a) exitosamente"));
        }

        private async Task<ActionResult<ApiResponse<TResponse>>> UpdateCatalogAsync<TEntity, TResponse>(
            int id,
            Expression<Func<TEntity, bool>> duplicateCheck,
            Action<TEntity> updateEntity,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken)
            where TEntity : class
            where TResponse : class
        {
            var entity = await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<TResponse>.NotFound(entityDisplayName));

            var duplicate = await _context.Set<TEntity>().AnyAsync(duplicateCheck, cancellationToken);
            if (duplicate)
            {
                return Conflict(ApiResponse<TResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    $"Ya existe un(a) {entityDisplayName.ToLower()} con ese nombre"));
            }

            updateEntity(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<TResponse>.SuccessResult(toResponse(entity), $"{entityDisplayName} actualizado(a) exitosamente"));
        }

        private static ActivityTemplateTypeResponse MapActivityTemplateType(ActivityTemplateType e) => new()
        {
            Id = e.Id, SkillAreaId = e.SkillAreaId, Name = e.Name, Code = e.Code,
            Description = e.Description, ContentSchema = e.ContentSchema,
            ComponentName = e.ComponentName, UsesPictograms = e.UsesPictograms,
            HasAudio = e.HasAudio, DisplayOrder = e.DisplayOrder
        };

        #endregion
    }
}
