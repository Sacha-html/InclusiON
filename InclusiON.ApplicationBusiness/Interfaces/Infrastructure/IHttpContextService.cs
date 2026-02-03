namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
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
    }
}
