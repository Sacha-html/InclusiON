using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Data;
using InclusiON.Domain.Models;
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
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateDisabilityType(
            [FromBody] CreateDisabilityTypeRequest request,
            CancellationToken cancellationToken)
        {
            var exists = await _context.Set<DisabilityType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (exists)
            {
                return Conflict(ApiResponse<CatalogItemResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un tipo de discapacidad con el nombre '{request.Name}'"));
            }

            var entity = new DisabilityType
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };

            _context.Set<DisabilityType>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new CatalogItemResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CatalogItemResponse>.SuccessResult(response, "Tipo de discapacidad creado exitosamente"));
        }

        /// <summary>
        /// Actualiza un tipo de discapacidad existente.
        /// </summary>
        [HttpPut("disability-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateDisabilityType(
            int id,
            [FromBody] UpdateDisabilityTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<DisabilityType>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<CatalogItemResponse>.NotFound("Tipo de discapacidad"));
            }

            var duplicate = await _context.Set<DisabilityType>()
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<CatalogItemResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un tipo de discapacidad con el nombre '{request.Name}'"));
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new CatalogItemResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };

            return Ok(ApiResponse<CatalogItemResponse>.SuccessResult(response, "Tipo de discapacidad actualizado exitosamente"));
        }

        #endregion

        #region Autonomy Levels

        /// <summary>
        /// Crea un nuevo nivel de autonomia.
        /// </summary>
        [HttpPost("autonomy-levels")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> CreateAutonomyLevel(
            [FromBody] CreateAutonomyLevelRequest request,
            CancellationToken cancellationToken)
        {
            var exists = await _context.Set<AutonomyLevel>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (exists)
            {
                return Conflict(ApiResponse<AutonomyLevelResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un nivel de autonomia con el nombre '{request.Name}'"));
            }

            var entity = new AutonomyLevel
            {
                Name = request.Name,
                Description = request.Description,
                RequiresSupervision = request.RequiresSupervision,
                DisplayOrder = request.DisplayOrder,
                IsActive = true
            };

            _context.Set<AutonomyLevel>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AutonomyLevelResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                RequiresSupervision = entity.RequiresSupervision,
                DisplayOrder = entity.DisplayOrder
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AutonomyLevelResponse>.SuccessResult(response, "Nivel de autonomia creado exitosamente"));
        }

        /// <summary>
        /// Actualiza un nivel de autonomia existente.
        /// </summary>
        [HttpPut("autonomy-levels/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> UpdateAutonomyLevel(
            int id,
            [FromBody] UpdateAutonomyLevelRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<AutonomyLevel>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<AutonomyLevelResponse>.NotFound("Nivel de autonomia"));
            }

            var duplicate = await _context.Set<AutonomyLevel>()
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<AutonomyLevelResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un nivel de autonomia con el nombre '{request.Name}'"));
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.RequiresSupervision = request.RequiresSupervision;
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new AutonomyLevelResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                RequiresSupervision = entity.RequiresSupervision,
                DisplayOrder = entity.DisplayOrder
            };

            return Ok(ApiResponse<AutonomyLevelResponse>.SuccessResult(response, "Nivel de autonomia actualizado exitosamente"));
        }

        #endregion

        #region Activity Categories

        /// <summary>
        /// Crea una nueva categoria de actividad.
        /// </summary>
        [HttpPost("activity-categories")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateActivityCategory(
            [FromBody] CreateActivityCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var exists = await _context.Set<ActivityCategory>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (exists)
            {
                return Conflict(ApiResponse<CatalogItemResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe una categoria de actividad con el nombre '{request.Name}'"));
            }

            var entity = new ActivityCategory
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };

            _context.Set<ActivityCategory>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new CatalogItemResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CatalogItemResponse>.SuccessResult(response, "Categoria de actividad creada exitosamente"));
        }

        /// <summary>
        /// Actualiza una categoria de actividad existente.
        /// </summary>
        [HttpPut("activity-categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateActivityCategory(
            int id,
            [FromBody] UpdateActivityCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<ActivityCategory>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<CatalogItemResponse>.NotFound("Categoria de actividad"));
            }

            var duplicate = await _context.Set<ActivityCategory>()
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<CatalogItemResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe una categoria de actividad con el nombre '{request.Name}'"));
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new CatalogItemResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };

            return Ok(ApiResponse<CatalogItemResponse>.SuccessResult(response, "Categoria de actividad actualizada exitosamente"));
        }

        #endregion

        #region Skill Areas

        /// <summary>
        /// Crea una nueva area de habilidad.
        /// </summary>
        [HttpPost("skill-areas")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SkillAreaResponse>>> CreateSkillArea(
            [FromBody] CreateSkillAreaRequest request,
            CancellationToken cancellationToken)
        {
            var exists = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (exists)
            {
                return Conflict(ApiResponse<SkillAreaResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un area de habilidad con el nombre '{request.Name}'"));
            }

            var entity = new SkillArea
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                Color = request.Color,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<SkillArea>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new SkillAreaResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Icon = entity.Icon,
                Color = entity.Color,
                DisplayOrder = entity.DisplayOrder
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<SkillAreaResponse>.SuccessResult(response, "Area de habilidad creada exitosamente"));
        }

        /// <summary>
        /// Actualiza un area de habilidad existente.
        /// </summary>
        [HttpPut("skill-areas/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SkillAreaResponse>>> UpdateSkillArea(
            int id,
            [FromBody] UpdateSkillAreaRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<SkillArea>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<SkillAreaResponse>.NotFound("Area de habilidad"));
            }

            var duplicate = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<SkillAreaResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un area de habilidad con el nombre '{request.Name}'"));
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Icon = request.Icon;
            entity.Color = request.Color;
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new SkillAreaResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Icon = entity.Icon,
                Color = entity.Color,
                DisplayOrder = entity.DisplayOrder
            };

            return Ok(ApiResponse<SkillAreaResponse>.SuccessResult(response, "Area de habilidad actualizada exitosamente"));
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
            [FromBody] CreateActivityTemplateTypeRequest request,
            CancellationToken cancellationToken)
        {
            var skillAreaExists = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Id == request.SkillAreaId, cancellationToken);

            if (!skillAreaExists)
            {
                return NotFound(ApiResponse<ActivityTemplateTypeResponse>.NotFound("Area de habilidad"));
            }

            var exists = await _context.Set<ActivityTemplateType>()
                .AnyAsync(x => x.Name == request.Name || x.Code == request.Code, cancellationToken);

            if (exists)
            {
                return Conflict(ApiResponse<ActivityTemplateTypeResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un tipo de plantilla con el nombre '{request.Name}' o codigo '{request.Code}'"));
            }

            var entity = new ActivityTemplateType
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
            };

            _context.Set<ActivityTemplateType>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new ActivityTemplateTypeResponse
            {
                Id = entity.Id,
                SkillAreaId = entity.SkillAreaId,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                ContentSchema = entity.ContentSchema,
                ComponentName = entity.ComponentName,
                UsesPictograms = entity.UsesPictograms,
                HasAudio = entity.HasAudio,
                DisplayOrder = entity.DisplayOrder
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ActivityTemplateTypeResponse>.SuccessResult(response, "Tipo de plantilla de actividad creado exitosamente"));
        }

        /// <summary>
        /// Actualiza un tipo de plantilla de actividad existente.
        /// </summary>
        [HttpPut("activity-template-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> UpdateActivityTemplateType(
            int id,
            [FromBody] UpdateActivityTemplateTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<ActivityTemplateType>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<ActivityTemplateTypeResponse>.NotFound("Tipo de plantilla de actividad"));
            }

            var skillAreaExists = await _context.Set<SkillArea>()
                .AnyAsync(x => x.Id == request.SkillAreaId, cancellationToken);

            if (!skillAreaExists)
            {
                return NotFound(ApiResponse<ActivityTemplateTypeResponse>.NotFound("Area de habilidad"));
            }

            var duplicate = await _context.Set<ActivityTemplateType>()
                .AnyAsync(x => (x.Name == request.Name || x.Code == request.Code) && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<ActivityTemplateTypeResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un tipo de plantilla con el nombre '{request.Name}' o codigo '{request.Code}'"));
            }

            entity.SkillAreaId = request.SkillAreaId;
            entity.Name = request.Name;
            entity.Code = request.Code;
            entity.Description = request.Description;
            entity.ContentSchema = request.ContentSchema;
            entity.ComponentName = request.ComponentName;
            entity.UsesPictograms = request.UsesPictograms;
            entity.HasAudio = request.HasAudio;
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ActivityTemplateTypeResponse
            {
                Id = entity.Id,
                SkillAreaId = entity.SkillAreaId,
                Name = entity.Name,
                Code = entity.Code,
                Description = entity.Description,
                ContentSchema = entity.ContentSchema,
                ComponentName = entity.ComponentName,
                UsesPictograms = entity.UsesPictograms,
                HasAudio = entity.HasAudio,
                DisplayOrder = entity.DisplayOrder
            };

            return Ok(ApiResponse<ActivityTemplateTypeResponse>.SuccessResult(response, "Tipo de plantilla de actividad actualizado exitosamente"));
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
        public async Task<ActionResult<ApiResponse<LoginMethodResponse>>> UpdateLoginMethod(
            int id,
            [FromBody] UpdateLoginMethodCatalogRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _context.Set<LoginMethod>()
                .FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<LoginMethodResponse>.NotFound("Metodo de login"));
            }

            var duplicate = await _context.Set<LoginMethod>()
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (duplicate)
            {
                return Conflict(ApiResponse<LoginMethodResponse>.Conflict(
                    DTOs.Common.ErrorCode.DuplicateEntry,
                    $"Ya existe un metodo de login con el nombre '{request.Name}'"));
            }

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            var response = new LoginMethodResponse
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                RequiresPassword = entity.RequiresPassword,
                RequiresPin = entity.RequiresPin,
                RequiresSupervisor = entity.RequiresSupervisor,
                DisplayOrder = entity.DisplayOrder
            };

            return Ok(ApiResponse<LoginMethodResponse>.SuccessResult(response, "Metodo de login actualizado exitosamente"));
        }

        #endregion
    }
}
