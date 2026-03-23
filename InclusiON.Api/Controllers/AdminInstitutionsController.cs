using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Controllers
{
    [Route("api/admin/institutions-assignments")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(Policy = "settings:update")]
    public class AdminInstitutionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextService _httpContextService;

        public AdminInstitutionsController(
            AppDbContext context,
            IHttpContextService httpContextService)
        {
            _context = context;
            _httpContextService = httpContextService;
        }

        /// <summary>
        /// Obtiene las instituciones asignadas al usuario administrador autenticado.
        /// Lista vacia indica administrador global.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<AdminInstitutionResponse>>>> GetMyInstitutions(
            CancellationToken cancellationToken)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(ApiResponse<List<AdminInstitutionResponse>>.Unauthorized());
            }

            var assignments = await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .Where(ai => ai.AdminUserId == userId.Value && ai.IsActive)
                .Select(ai => new AdminInstitutionResponse
                {
                    AdminUserId = ai.AdminUserId,
                    InstitutionId = ai.InstitutionId,
                    InstitutionName = ai.Institution.Name,
                    AssignedAt = ai.AssignedAt,
                    IsActive = ai.IsActive
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<AdminInstitutionResponse>>.SuccessResult(assignments));
        }

        /// <summary>
        /// Obtiene las instituciones asignadas a un administrador especifico.
        /// </summary>
        [HttpGet("{adminUserId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminInstitutionResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminInstitutionResponse>>>> GetAdminInstitutions(
            Guid adminUserId,
            CancellationToken cancellationToken)
        {
            var assignments = await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .Where(ai => ai.AdminUserId == adminUserId)
                .Select(ai => new AdminInstitutionResponse
                {
                    AdminUserId = ai.AdminUserId,
                    InstitutionId = ai.InstitutionId,
                    InstitutionName = ai.Institution.Name,
                    AssignedAt = ai.AssignedAt,
                    IsActive = ai.IsActive
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<AdminInstitutionResponse>>.SuccessResult(assignments));
        }

        /// <summary>
        /// Asigna una institucion a un administrador.
        /// </summary>
        [HttpPost("{adminUserId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> AssignInstitution(
            Guid adminUserId,
            [FromBody] AssignInstitutionRequest request,
            CancellationToken cancellationToken)
        {
            // Verificar que el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == adminUserId, cancellationToken);
            if (!userExists)
            {
                return NotFound(ApiResponse<AdminInstitutionResponse>.NotFound("Usuario"));
            }

            // Verificar que la institucion existe
            var institution = await _context.EducationalInstitutions
                .FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken);
            if (institution == null)
            {
                return NotFound(ApiResponse<AdminInstitutionResponse>.NotFound("Institucion"));
            }

            // Verificar si ya existe la asignacion
            var existing = await _context.AdminInstitutions
                .FirstOrDefaultAsync(ai => ai.AdminUserId == adminUserId && ai.InstitutionId == request.InstitutionId, cancellationToken);

            if (existing != null)
            {
                // Reactivar si estaba inactiva
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.AssignedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var reactivatedResponse = new AdminInstitutionResponse
                {
                    AdminUserId = existing.AdminUserId,
                    InstitutionId = existing.InstitutionId,
                    InstitutionName = institution.Name,
                    AssignedAt = existing.AssignedAt,
                    IsActive = existing.IsActive
                };

                return Ok(ApiResponse<AdminInstitutionResponse>.SuccessResult(reactivatedResponse, "Asignacion creada exitosamente"));
            }

            var adminInstitution = new AdminInstitution
            {
                AdminUserId = adminUserId,
                InstitutionId = request.InstitutionId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AdminInstitutions.Add(adminInstitution);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AdminInstitutionResponse
            {
                AdminUserId = adminInstitution.AdminUserId,
                InstitutionId = adminInstitution.InstitutionId,
                InstitutionName = institution.Name,
                AssignedAt = adminInstitution.AssignedAt,
                IsActive = adminInstitution.IsActive
            };

            return Ok(ApiResponse<AdminInstitutionResponse>.SuccessResult(response, "Asignacion creada exitosamente"));
        }

        /// <summary>
        /// Elimina la asignacion de una institucion a un administrador.
        /// </summary>
        [HttpDelete("{adminUserId:guid}/{institutionId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminInstitutionResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminInstitutionResponse>>> RemoveAssignment(
            Guid adminUserId,
            int institutionId,
            CancellationToken cancellationToken)
        {
            var assignment = await _context.AdminInstitutions
                .Include(ai => ai.Institution)
                .FirstOrDefaultAsync(ai => ai.AdminUserId == adminUserId && ai.InstitutionId == institutionId, cancellationToken);

            if (assignment == null)
            {
                return NotFound(ApiResponse<AdminInstitutionResponse>.NotFound("Asignacion"));
            }

            _context.AdminInstitutions.Remove(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AdminInstitutionResponse
            {
                AdminUserId = assignment.AdminUserId,
                InstitutionId = assignment.InstitutionId,
                InstitutionName = assignment.Institution.Name,
                AssignedAt = assignment.AssignedAt,
                IsActive = assignment.IsActive
            };

            return Ok(ApiResponse<AdminInstitutionResponse>.SuccessResult(response, "Asignacion eliminada exitosamente"));
        }
    }

    /// <summary>
    /// Request para asignar una institucion a un administrador.
    /// </summary>
    public class AssignInstitutionRequest
    {
        public int InstitutionId { get; set; }
    }
}
