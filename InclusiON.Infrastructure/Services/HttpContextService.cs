using Microsoft.AspNetCore.Http;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;

namespace InclusiON.Infrastructure.Services
{
    /// <summary>
    /// Implementacion del servicio para extraer informacion del contexto HTTP.
    /// </summary>
    public class HttpContextService : IHttpContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string? GetClientIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context is null)
            {
                return null;
            }

            // 1. Verificar X-Forwarded-For (proxies, load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }

            // 2. Verificar X-Client-IP (algunos proxies)
            var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientIp))
            {
                return clientIp;
            }

            // 3. Usar la IP de la conexion directa
            return context.Connection.RemoteIpAddress?.ToString();
        }

        /// <inheritdoc />
        public string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        }

        /// <inheritdoc />
        public string? ParseBrowserFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return null;
            }

            // Orden importante: Edge contiene "Chrome", Safari puede estar en Chrome
            if (userAgent.Contains("Edg"))
            {
                return "Edge";
            }

            if (userAgent.Contains("Chrome"))
            {
                return "Chrome";
            }

            if (userAgent.Contains("Firefox"))
            {
                return "Firefox";
            }

            if (userAgent.Contains("Safari"))
            {
                return "Safari";
            }

            if (userAgent.Contains("Opera") || userAgent.Contains("OPR"))
            {
                return "Opera";
            }

            return "Other";
        }
    }
}
