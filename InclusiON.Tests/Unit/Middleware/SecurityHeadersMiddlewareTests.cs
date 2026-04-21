using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using InclusiON.Api.Middleware;

namespace InclusiON.Tests.Unit.Middleware
{
    public class SecurityHeadersMiddlewareTests
    {
        private static async Task<HttpContext> InvokeAsync(RequestDelegate? next = null)
        {
            var context    = new DefaultHttpContext();
            var middleware = new SecurityHeadersMiddleware(next ?? (_ => Task.CompletedTask));
            await middleware.InvokeAsync(context);
            return context;
        }

        // ── Headers presentes ────────────────────────────────────────────────

        [Fact]
        public async Task InvokeAsync_SetsXContentTypeOptions_Nosniff()
        {
            var ctx = await InvokeAsync();
            ctx.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        }

        [Fact]
        public async Task InvokeAsync_SetsXFrameOptions_Deny()
        {
            var ctx = await InvokeAsync();
            ctx.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        }

        [Fact]
        public async Task InvokeAsync_SetsReferrerPolicy()
        {
            var ctx = await InvokeAsync();
            ctx.Response.Headers["Referrer-Policy"].ToString()
               .Should().Be("strict-origin-when-cross-origin");
        }

        [Fact]
        public async Task InvokeAsync_SetsPermissionsPolicy_DisablesHardwareApis()
        {
            var ctx   = await InvokeAsync();
            var value = ctx.Response.Headers["Permissions-Policy"].ToString();

            value.Should().Contain("camera=()");
            value.Should().Contain("microphone=()");
            value.Should().Contain("geolocation=()");
        }

        [Fact]
        public async Task InvokeAsync_SetsXXssProtection_Zero()
        {
            // 0 deshabilita el filtro XSS legacy del browser (OWASP recomienda deshabilitar)
            var ctx = await InvokeAsync();
            ctx.Response.Headers["X-XSS-Protection"].ToString().Should().Be("0");
        }

        // ── Pipeline ────────────────────────────────────────────────────────

        [Fact]
        public async Task InvokeAsync_CallsNextMiddleware()
        {
            var nextCalled = false;
            RequestDelegate next = async ctx =>
            {
                nextCalled = true;
                await ctx.Response.StartAsync();
            };

            await InvokeAsync(next);

            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_NextMiddlewareThrows_ExceptionPropagates()
        {
            RequestDelegate next = _ => throw new InvalidOperationException("downstream error");
            var middleware = new SecurityHeadersMiddleware(next);
            var context    = new DefaultHttpContext();

            var act = async () => await middleware.InvokeAsync(context);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("downstream error");
        }

        [Fact]
        public async Task InvokeAsync_AllFiveHeadersArePresent()
        {
            var ctx      = await InvokeAsync();
            var expected = new[]
            {
                "X-Content-Type-Options",
                "X-Frame-Options",
                "Referrer-Policy",
                "Permissions-Policy",
                "X-XSS-Protection"
            };

            foreach (var header in expected)
            {
                ctx.Response.Headers.ContainsKey(header)
                   .Should().BeTrue(because: $"el header {header} debe estar presente");
            }
        }
    }
}
