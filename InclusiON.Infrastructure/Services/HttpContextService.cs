using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;

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

        /// <inheritdoc />
        public Guid? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return null;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return null;
            }

            return userId;
        }

        /// <inheritdoc />
        public string? GetCurrentUserRole()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return null;
            }

            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <inheritdoc />
        public string? GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.TraceIdentifier;
        }

        /// <inheritdoc />
        public bool IsGlobalAdmin()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return false;

            var claim = user.FindFirst(Permissions.GlobalAdminClaimType);
            return claim is not null && claim.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public List<int> GetInstitutionIds()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null) return new List<int>();

            return user.FindAll(Permissions.InstitutionIdClaimType)
                .Select(c => int.TryParse(c.Value, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }
    }
}
