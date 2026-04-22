namespace InclusiON.Api.Middleware;

// Agrega headers de seguridad HTTP a todas las respuestas.
// X-XSS-Protection se desactiva intencionalmente (valor 0): habilitarlo abre vectores de ataque
// según OWASP; la protección correcta es via Content-Security-Policy.
// TODO: agregar CSP una vez el frontend esté estabilizado (usar Report-Only primero para auditar).
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"]  = "nosniff";
        headers["X-Frame-Options"]          = "DENY";
        headers["Referrer-Policy"]          = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"]       = "camera=(), microphone=(), geolocation=()";
        headers["X-XSS-Protection"]         = "0";

        await next(context);
    }
}
