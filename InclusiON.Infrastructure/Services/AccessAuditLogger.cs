using Microsoft.Extensions.Logging;
using InclusiON.Application.Auditing;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementacion sincrona de <see cref="IAccessAuditLogger"/>.
    /// Persiste cada evento directamente en la tabla <c>AccessAudits</c>.
    ///
    /// Decision de diseño (HU-IN-172): se prefiere persistencia sincrona inline sobre un worker
    /// en background porque: (1) el volumen esperado es bajo (~2k eventos/dia),
    /// (2) cualquier perdida por crash seria inaceptable legalmente (Ley 25.326),
    /// (3) el INSERT se suma a queries que el handler ya estaba haciendo.
    /// </summary>
    public class AccessAuditLogger : IAccessAuditLogger
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextService _httpContextService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AccessAuditLogger> _logger;

        public AccessAuditLogger(
            AppDbContext context,
            IHttpContextService httpContextService,
            IDateTimeProvider dateTimeProvider,
            ILogger<AccessAuditLogger> logger)
        {
            _context = context;
            _httpContextService = httpContextService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task LogAsync(AccessAuditEntry entry, CancellationToken cancellationToken = default)
        {
            try
            {
                var audit = new AccessAudit
                {
                    UserId = entry.UserId,
                    Role = entry.Role,
                    AccessedPersonId = entry.AccessedPersonId,
                    ActionType = entry.ActionType,
                    Result = entry.Result,
                    AffectedTable = entry.AffectedTable,
                    AffectedRecordId = entry.AffectedRecordId,
                    Details = entry.Details,
                    IpAddress = _httpContextService.GetClientIpAddress(),
                    CorrelationId = _httpContextService.GetCorrelationId(),
                    Timestamp = _dateTimeProvider.UtcNow
                };

                _context.AccessAudits.Add(audit);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // No re-lanzamos: un fallo en auditoria no debe romper el request principal.
                // Queda registrado en logs para investigacion.
                _logger.LogError(ex,
                    "Fallo al persistir AccessAudit para UserId={UserId} Action={Action} Result={Result}",
                    entry.UserId, entry.ActionType, entry.Result);
            }
        }
    }
}
