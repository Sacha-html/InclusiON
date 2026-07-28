using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio genérico para administración de catálogos.
    /// Centraliza duplicado, persistencia e integridad referencial.
    /// </summary>
    public class CatalogAdminService : ICatalogAdminService
    {
        private readonly AppDbContext _context;

        public CatalogAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AnyAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            return await _context.Set<TEntity>().AnyAsync(predicate, cancellationToken);
        }

        public async Task<ApiResponse<TResponse>> CreateAsync<TEntity, TResponse>(
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
                return ApiResponse<TResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    $"Ya existe un(a) {entityDisplayName.ToLower()} con ese nombre");
            }

            var entity = createEntity();
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<TResponse>.SuccessResult(
                toResponse(entity),
                $"{entityDisplayName} creado(a) exitosamente");
        }

        public async Task<ApiResponse<TResponse>> UpdateAsync<TEntity, TResponse>(
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
            if (entity is null)
                return ApiResponse<TResponse>.NotFound(entityDisplayName);

            var duplicate = await _context.Set<TEntity>().AnyAsync(duplicateCheck, cancellationToken);
            if (duplicate)
            {
                return ApiResponse<TResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    $"Ya existe un(a) {entityDisplayName.ToLower()} con ese nombre");
            }

            updateEntity(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<TResponse>.SuccessResult(
                toResponse(entity),
                $"{entityDisplayName} actualizado(a) exitosamente");
        }

        public async Task<ApiResponse<TResponse>> PatchStatusAsync<TEntity, TResponse>(
            int id,
            bool requestedIsActive,
            Func<TEntity, bool> getIsActive,
            Action<TEntity, bool> applyStatus,
            Func<TEntity, TResponse> toResponse,
            string entityDisplayName,
            CancellationToken cancellationToken,
            Func<int, CancellationToken, Task<string?>>? deactivationCheck = null)
            where TEntity : class
            where TResponse : class
        {
            var entity = await _context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return ApiResponse<TResponse>.NotFound(entityDisplayName);

            // Rechazar transición no-op
            if (getIsActive(entity) == requestedIsActive)
            {
                var estado = requestedIsActive ? "activo(a)" : "inactivo(a)";
                return ApiResponse<TResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    $"El(La) {entityDisplayName.ToLower()} ya se encuentra {estado}.");
            }

            // Transición activo → inactivo: validar integridad referencial
            if (!requestedIsActive && deactivationCheck is not null)
            {
                var error = await deactivationCheck(id, cancellationToken);
                if (error is not null)
                    return ApiResponse<TResponse>.Conflict(ErrorCode.BusinessRuleViolation, error);
            }

            applyStatus(entity, requestedIsActive);
            await _context.SaveChangesAsync(cancellationToken);

            var mensaje = requestedIsActive
                ? $"{entityDisplayName} reactivado(a) exitosamente."
                : $"{entityDisplayName} dado(a) de baja exitosamente.";

            return ApiResponse<TResponse>.SuccessResult(toResponse(entity), mensaje);
        }
    }
}
