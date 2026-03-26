using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Admin;
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
        private readonly IIdentityService _identityService;

        public AdminInstitutionsController(
            AppDbContext context,
            IHttpContextService httpContextService,
            IIdentityService identityService)
        {
            _context = context;
            _httpContextService = httpContextService;
            _identityService = identityService;
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios administradores con sus instituciones asignadas.
        /// </summary>
        [HttpGet("admins")]
        [Authorize(Policy = "global-admin")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminUserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminUserResponse>>>> GetAllAdmins(
            CancellationToken cancellationToken)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "ADMIN")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var admins = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId)
                .Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => u)
                .Select(u => new AdminUserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    Email = u.Email!,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Institutions = _context.AdminInstitutions
                        .Where(ai => ai.AdminUserId == u.Id && ai.IsActive)
                        .Select(ai => new AdminInstitutionInfo
                        {
                            InstitutionId = ai.InstitutionId,
                            InstitutionName = ai.Institution.Name
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            // Mark admins with no institutions as global admins
            foreach (var admin in admins)
            {
                admin.IsGlobalAdmin = admin.Institutions.Count == 0;
            }

            return Ok(ApiResponse<List<AdminUserResponse>>.SuccessResult(admins));
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

        /// <summary>
        /// Crea un nuevo usuario administrador y lo asigna a una institucion.
        /// </summary>
        [HttpPost("users")]
        [Authorize(Policy = "global-admin")]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<CreateAdminUserResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<CreateAdminUserResponse>>> CreateAdminUser(
            [FromBody] CreateAdminUserRequest request,
            CancellationToken cancellationToken)
        {
            // Verificar que la institucion existe
            var institution = await _context.EducationalInstitutions
                .FirstOrDefaultAsync(i => i.Id == request.InstitutionId, cancellationToken);
            if (institution == null)
            {
                return NotFound(ApiResponse<CreateAdminUserResponse>.NotFound("Institucion"));
            }

            // Verificar que no exista un usuario con ese email
            var existingUser = await _identityService.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(ApiResponse<CreateAdminUserResponse>.Conflict(
                    ErrorCode.EmailAlreadyExists,
                    "Ya existe un usuario con ese email."));
            }

            // Generar contrasena temporal
            var temporaryPassword = InclusiON.Application.Helpers.PasswordGenerator.GenerateTemporary();

            // Crear usuario
            var user = new User
            {
                Name = request.FirstName,
                Surname = request.LastName,
                Email = request.Email.ToLower(),
                UserName = request.Email.ToLower(),
                NormalizedEmail = request.Email.ToUpper(),
                NormalizedUserName = request.Email.ToUpper(),
                EmailConfirmed = true,
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _identityService.CreateUserAsync(user, temporaryPassword);
            if (!createResult.Succeeded)
            {
                return BadRequest(ApiResponse<CreateAdminUserResponse>.ErrorResult(
                    "Error al crear el usuario.",
                    createResult.Errors.ToList()));
            }

            // Asignar rol Admin
            var roleResult = await _identityService.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                return BadRequest(ApiResponse<CreateAdminUserResponse>.ErrorResult(
                    "Error al asignar el rol.",
                    roleResult.Errors.ToList()));
            }

            // Crear asignacion de institucion
            var adminInstitution = new AdminInstitution
            {
                AdminUserId = user.Id,
                InstitutionId = request.InstitutionId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.AdminInstitutions.Add(adminInstitution);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new CreateAdminUserResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = request.FirstName,
                LastName = request.LastName,
                InstitutionId = request.InstitutionId,
                InstitutionName = institution.Name,
                TemporaryPassword = temporaryPassword
            };

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CreateAdminUserResponse>.SuccessResult(response, "Usuario administrador creado exitosamente."));
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
