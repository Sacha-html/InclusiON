namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio para extraer informacion del contexto HTTP actual.
    /// </summary>
    public interface IHttpContextService
    {
        /// <summary>
        /// Obtiene la direccion IP del cliente, considerando proxies y balanceadores de carga.
        /// </summary>
        string? GetClientIpAddress();

        /// <summary>
        /// Obtiene el User-Agent del cliente.
        /// </summary>
        string? GetUserAgent();

        /// <summary>
        /// Extrae el nombre del navegador del User-Agent.
        /// </summary>
        string? ParseBrowserFromUserAgent(string? userAgent);

        /// <summary>
        /// Obtiene el ID del usuario autenticado a partir de los claims del token JWT.
        /// </summary>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Indica si el usuario autenticado es administrador global.
        /// </summary>
        bool IsGlobalAdmin();

        /// <summary>
        /// Obtiene los IDs de instituciones asignadas al admin desde los claims del JWT.
        /// Retorna lista vacia si no tiene claims de institucion.
        /// </summary>
        List<int> GetInstitutionIds();
    }
}
