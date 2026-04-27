using InclusiON.Application.Auditing;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio de auditoria de accesos a datos sensibles.
    /// Invocado desde <c>IResourceAuthorizationService</c> tras cada verificacion de autorizacion.
    /// </summary>
    public interface IAccessAuditLogger
    {
        /// <summary>
        /// Registra un evento de auditoria. El IP, CorrelationId y Timestamp se enriquecen
        /// desde <see cref="IHttpContextService"/> y <see cref="IDateTimeProvider"/>
        /// dentro de la implementacion.
        /// </summary>
        Task LogAsync(AccessAuditEntry entry, CancellationToken cancellationToken = default);
    }
}
