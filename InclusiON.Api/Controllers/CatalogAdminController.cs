using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using InclusiON.Api.Extensions;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Requests.Catalogs;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador de administracion de catalogos del sistema.
    /// Provee endpoints de escritura (crear/actualizar) para entidades de catalogo.
    /// Delega toda la lógica de negocio a <see cref="ICatalogAdminService"/>.
    /// </summary>
    [Route("api/admin/catalogs")]
    [ApiController]
    [Authorize(Policy = Permissions.Settings.Update)]
    [Authorize(Policy = Permissions.GlobalAdmin)]
    [Produces("application/json")]
    public class CatalogAdminController : ControllerBase
    {
        private readonly ICatalogAdminService _catalog;
        private readonly IOutputCacheStore _cacheStore;
        private readonly IMemoryCache _memoryCache;

        public CatalogAdminController(ICatalogAdminService catalog, IOutputCacheStore cacheStore, IMemoryCache memoryCache)
        {
            _catalog = catalog;
            _cacheStore = cacheStore;
            _memoryCache = memoryCache;
        }

        // ── helpers de ciclo de vida ──────────────────────────────────────────────

        private async Task InvalidateAllCachesAsync(CancellationToken ct)
        {
            await _cacheStore.EvictByTagAsync("catalogs", ct);
            foreach (var key in CatalogCacheKeys.All)
                _memoryCache.Remove(key);
        }

        private async Task<ActionResult<ApiResponse<T>>> CreatedAsync<T>(
            ApiResponse<T> result, CancellationToken ct) where T : class
        {
            if (!result.Success) return result.ToActionResult();
            await InvalidateAllCachesAsync(ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        private async Task<ActionResult<ApiResponse<T>>> OkAsync<T>(
            ApiResponse<T> result, CancellationToken ct) where T : class
        {
            if (result.Success)
                await InvalidateAllCachesAsync(ct);
            return result.ToActionResult();
        }

        #region Disability Types

        [HttpPost("disability-types")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateDisabilityType(
            [FromBody] CreateDisabilityTypeRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.CreateAsync<DisabilityType, CatalogItemResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new DisabilityType
                {
                    Name = request.Name, Description = request.Description, IsActive = true
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de discapacidad", cancellationToken);

            return await CreatedAsync(result, cancellationToken);
        }

        [HttpPut("disability-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateDisabilityType(
            int id, [FromBody] UpdateDisabilityTypeRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.UpdateAsync<DisabilityType, CatalogItemResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name;
                    e.Description = request.Description;
                    e.IsActive = request.IsActive;
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de discapacidad", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        [HttpPatch("disability-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> PatchDisabilityTypeStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<DisabilityType, CatalogItemResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => e.IsActive = v,
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de discapacidad", cancellationToken,
                deactivationCheck: async (typeId, ct) =>
                {
                    var hasPersons = await _catalog.AnyAsync<PersonWithDisability>(
                        x => x.DisabilityTypeId == typeId, ct);
                    return hasPersons
                        ? "No se puede dar de baja el tipo de discapacidad porque hay personas con discapacidad asociadas."
                        : null;
                });

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Autonomy Levels

        [HttpPost("autonomy-levels")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> CreateAutonomyLevel(
            [FromBody] CreateAutonomyLevelRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.CreateAsync<AutonomyLevel, AutonomyLevelResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new AutonomyLevel
                {
                    Name = request.Name, Description = request.Description,
                    RequiresSupervision = request.RequiresSupervision,
                    DisplayOrder = request.DisplayOrder, IsActive = true
                },
                toResponse: e => new AutonomyLevelResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    RequiresSupervision = e.RequiresSupervision, DisplayOrder = e.DisplayOrder
                },
                "Nivel de autonomia", cancellationToken);

            return await CreatedAsync(result, cancellationToken);
        }

        [HttpPut("autonomy-levels/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> UpdateAutonomyLevel(
            int id, [FromBody] UpdateAutonomyLevelRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.UpdateAsync<AutonomyLevel, AutonomyLevelResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name; e.Description = request.Description;
                    e.RequiresSupervision = request.RequiresSupervision;
                    e.DisplayOrder = request.DisplayOrder; e.IsActive = request.IsActive;
                },
                toResponse: e => new AutonomyLevelResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    RequiresSupervision = e.RequiresSupervision, DisplayOrder = e.DisplayOrder
                },
                "Nivel de autonomia", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        [HttpPatch("autonomy-levels/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<AutonomyLevelResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AutonomyLevelResponse>>> PatchAutonomyLevelStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<AutonomyLevel, AutonomyLevelResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => e.IsActive = v,
                toResponse: e => new AutonomyLevelResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    RequiresSupervision = e.RequiresSupervision, DisplayOrder = e.DisplayOrder
                },
                "Nivel de autonomia", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Activity Categories

        [HttpPost("activity-categories")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> CreateActivityCategory(
            [FromBody] CreateActivityCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.CreateAsync<ActivityCategory, CatalogItemResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new ActivityCategory
                {
                    Name = request.Name, Description = request.Description, IsActive = true
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Categoria de actividad", cancellationToken);

            return await CreatedAsync(result, cancellationToken);
        }

        [HttpPut("activity-categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> UpdateActivityCategory(
            int id, [FromBody] UpdateActivityCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.UpdateAsync<ActivityCategory, CatalogItemResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name; e.Description = request.Description; e.IsActive = request.IsActive;
                },
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Categoria de actividad", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        [HttpPatch("activity-categories/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> PatchActivityCategoryStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<ActivityCategory, CatalogItemResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => e.IsActive = v,
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Categoria de actividad", cancellationToken,
                deactivationCheck: async (categoryId, ct) =>
                {
                    var hasActivities = await _catalog.AnyAsync<Activity>(
                        x => x.CategoryId == categoryId, ct);
                    return hasActivities
                        ? "No se puede dar de baja la categoria porque tiene actividades asociadas."
                        : null;
                });

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Skill Areas

        [HttpPost("skill-areas")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SkillAreaResponse>>> CreateSkillArea(
            [FromBody] CreateSkillAreaRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.CreateAsync<SkillArea, SkillAreaResponse>(
                duplicateCheck: x => x.Name == request.Name,
                createEntity: () => new SkillArea
                {
                    Name = request.Name, Description = request.Description,
                    Icon = request.Icon, Color = request.Color,
                    DisplayOrder = request.DisplayOrder, IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                toResponse: e => new SkillAreaResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    Icon = e.Icon, Color = e.Color, DisplayOrder = e.DisplayOrder
                },
                "Area de habilidad", cancellationToken);

            return await CreatedAsync(result, cancellationToken);
        }

        [HttpPut("skill-areas/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SkillAreaResponse>>> UpdateSkillArea(
            int id, [FromBody] UpdateSkillAreaRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.UpdateAsync<SkillArea, SkillAreaResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name; e.Description = request.Description;
                    e.Icon = request.Icon; e.Color = request.Color;
                    e.DisplayOrder = request.DisplayOrder; e.IsActive = request.IsActive;
                    e.UpdatedAt = DateTime.UtcNow;
                },
                toResponse: e => new SkillAreaResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    Icon = e.Icon, Color = e.Color, DisplayOrder = e.DisplayOrder
                },
                "Area de habilidad", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        [HttpPatch("skill-areas/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<SkillAreaResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<SkillAreaResponse>>> PatchSkillAreaStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<SkillArea, SkillAreaResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => { e.IsActive = v; e.UpdatedAt = DateTime.UtcNow; },
                toResponse: e => new SkillAreaResponse
                {
                    Id = e.Id, Name = e.Name, Description = e.Description,
                    Icon = e.Icon, Color = e.Color, DisplayOrder = e.DisplayOrder
                },
                "Area de habilidad", cancellationToken,
                deactivationCheck: async (areaId, ct) =>
                {
                    if (await _catalog.AnyAsync<PersonSkillProfile>(x => x.SkillAreaId == areaId && x.IsActive, ct))
                        return "No se puede dar de baja el area porque tiene perfiles de habilidad activos asociados.";

                    if (await _catalog.AnyAsync<ActivityTemplateType>(x => x.SkillAreaId == areaId && x.IsActive, ct))
                        return "No se puede dar de baja el area porque tiene tipos de plantilla activos asociados.";

                    return null;
                });

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Activity Template Types

        [HttpPost("activity-template-types")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> CreateActivityTemplateType(
            [FromBody] CreateActivityTemplateTypeRequest request, CancellationToken cancellationToken)
        {
            var skillAreaExists = await _catalog.AnyAsync<SkillArea>(
                x => x.Id == request.SkillAreaId && x.IsActive, cancellationToken);

            if (!skillAreaExists)
                return ApiResponse<ActivityTemplateTypeResponse>
                    .NotFound("Area de habilidad")
                    .ToActionResult();

            var result = await _catalog.CreateAsync<ActivityTemplateType, ActivityTemplateTypeResponse>(
                duplicateCheck: x => x.Name == request.Name || x.Code == request.Code,
                createEntity: () => new ActivityTemplateType
                {
                    SkillAreaId    = request.SkillAreaId,
                    Name           = request.Name,
                    Code           = request.Code,
                    Description    = request.Description,
                    ContentSchema  = request.ContentSchema,
                    ComponentName  = request.ComponentName,
                    UsesPictograms = request.UsesPictograms,
                    HasAudio       = request.HasAudio,
                    DisplayOrder   = request.DisplayOrder,
                    IsActive       = true
                },
                toResponse: MapActivityTemplateType,
                "Tipo de plantilla de actividad", cancellationToken);

            return await CreatedAsync(result, cancellationToken);
        }

        [HttpPut("activity-template-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> UpdateActivityTemplateType(
            int id, [FromBody] UpdateActivityTemplateTypeRequest request, CancellationToken cancellationToken)
        {
            var skillAreaExists = await _catalog.AnyAsync<SkillArea>(
                x => x.Id == request.SkillAreaId && x.IsActive, cancellationToken);

            if (!skillAreaExists)
                return ApiResponse<ActivityTemplateTypeResponse>
                    .NotFound("Area de habilidad")
                    .ToActionResult();

            var result = await _catalog.UpdateAsync<ActivityTemplateType, ActivityTemplateTypeResponse>(
                id,
                duplicateCheck: x => (x.Name == request.Name || x.Code == request.Code) && x.Id != id,
                updateEntity: e =>
                {
                    e.SkillAreaId    = request.SkillAreaId;
                    e.Name           = request.Name;
                    e.Code           = request.Code;
                    e.Description    = request.Description;
                    e.ContentSchema  = request.ContentSchema;
                    e.ComponentName  = request.ComponentName;
                    e.UsesPictograms = request.UsesPictograms;
                    e.HasAudio       = request.HasAudio;
                    e.DisplayOrder   = request.DisplayOrder;
                    e.IsActive       = request.IsActive;
                },
                toResponse: MapActivityTemplateType,
                "Tipo de plantilla de actividad", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        [HttpPatch("activity-template-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ActivityTemplateTypeResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ActivityTemplateTypeResponse>>> PatchActivityTemplateTypeStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<ActivityTemplateType, ActivityTemplateTypeResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => { e.IsActive = v; e.UpdatedAt = DateTime.UtcNow; },
                toResponse: MapActivityTemplateType,
                "Tipo de plantilla de actividad", cancellationToken,
                deactivationCheck: async (templateTypeId, ct) =>
                {
                    var hasContents = await _catalog.AnyAsync<ActivityContent>(
                        x => x.TemplateTypeId == templateTypeId, ct);
                    return hasContents
                        ? "No se puede dar de baja el tipo de plantilla porque tiene contenidos de actividad asociados."
                        : null;
                });

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Report Types

        [HttpPatch("report-types/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CatalogItemResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CatalogItemResponse>>> PatchReportTypeStatus(
            int id, [FromBody] PatchStatusRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.PatchStatusAsync<ReportType, CatalogItemResponse>(
                id, request.IsActive,
                getIsActive: e => e.IsActive,
                applyStatus: (e, v) => e.IsActive = v,
                toResponse: e => new CatalogItemResponse { Id = e.Id, Name = e.Name, Description = e.Description },
                "Tipo de reporte", cancellationToken,
                deactivationCheck: async (typeId, ct) =>
                {
                    var hasReports = await _catalog.AnyAsync<Report>(
                        x => x.ReportTypeId == typeId, ct);
                    return hasReports
                        ? "No se puede dar de baja el tipo de reporte porque tiene reportes asociados."
                        : null;
                });

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        #region Login Methods

        [HttpPut("login-methods/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<LoginMethodResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<LoginMethodResponse>>> UpdateLoginMethod(
            int id, [FromBody] UpdateLoginMethodCatalogRequest request, CancellationToken cancellationToken)
        {
            var result = await _catalog.UpdateAsync<LoginMethod, LoginMethodResponse>(
                id,
                duplicateCheck: x => x.Name == request.Name && x.Id != id,
                updateEntity: e =>
                {
                    e.Name = request.Name; e.Description = request.Description;
                    e.DisplayOrder = request.DisplayOrder; e.IsActive = request.IsActive;
                },
                toResponse: e => new LoginMethodResponse
                {
                    Id = e.Id, Code = e.Code, Name = e.Name, Description = e.Description,
                    RequiresPassword = e.RequiresPassword, RequiresPin = e.RequiresPin,
                    RequiresSupervisor = e.RequiresSupervisor, DisplayOrder = e.DisplayOrder
                },
                "Metodo de login", cancellationToken);

            return await OkAsync(result, cancellationToken);
        }

        #endregion

        private static ActivityTemplateTypeResponse MapActivityTemplateType(ActivityTemplateType e) => new()
        {
            Id = e.Id, SkillAreaId = e.SkillAreaId, Name = e.Name, Code = e.Code,
            Description = e.Description, ContentSchema = e.ContentSchema,
            ComponentName = e.ComponentName, UsesPictograms = e.UsesPictograms,
            HasAudio = e.HasAudio, DisplayOrder = e.DisplayOrder
        };
    }
}
