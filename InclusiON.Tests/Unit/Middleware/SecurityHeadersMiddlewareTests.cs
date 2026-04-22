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
            // Arrange
            // (middleware built inside InvokeAsync helper)

            // Act
            var ctx = await InvokeAsync();

            // Assert
            ctx.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        }

        [Fact]
        public async Task InvokeAsync_SetsXFrameOptions_Deny()
        {
            // Arrange
            // (middleware built inside InvokeAsync helper)

            // Act
            var ctx = await InvokeAsync();

            // Assert
            ctx.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        }

        [Fact]
        public async Task InvokeAsync_SetsReferrerPolicy()
        {
            // Arrange
            // (middleware built inside InvokeAsync helper)

            // Act
            var ctx = await InvokeAsync();

            // Assert
            ctx.Response.Headers["Referrer-Policy"].ToString()
               .Should().Be("strict-origin-when-cross-origin");
        }

        [Fact]
        public async Task InvokeAsync_SetsPermissionsPolicy_DisablesHardwareApis()
        {
            // Arrange
            // (middleware built inside InvokeAsync helper)

            // Act
            var ctx   = await InvokeAsync();
            var value = ctx.Response.Headers["Permissions-Policy"].ToString();

            // Assert
            value.Should().Contain("camera=()");
            value.Should().Contain("microphone=()");
            value.Should().Contain("geolocation=()");
        }

        [Fact]
        public async Task InvokeAsync_SetsXXssProtection_Zero()
        {
            // Arrange
            // 0 deshabilita el filtro XSS legacy del browser (OWASP recomienda deshabilitar)

            // Act
            var ctx = await InvokeAsync();

            // Assert
            ctx.Response.Headers["X-XSS-Protection"].ToString().Should().Be("0");
        }

        // ── Pipeline ────────────────────────────────────────────────────────

        [Fact]
        public async Task InvokeAsync_CallsNextMiddleware()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = async ctx =>
            {
                nextCalled = true;
                await ctx.Response.StartAsync();
            };

            // Act
            await InvokeAsync(next);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_NextMiddlewareThrows_ExceptionPropagates()
        {
            // Arrange
            RequestDelegate next = _ => throw new InvalidOperationException("downstream error");
            var middleware = new SecurityHeadersMiddleware(next);
            var context    = new DefaultHttpContext();
            var act = async () => await middleware.InvokeAsync(context);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("downstream error");
        }

        [Fact]
        public async Task InvokeAsync_AllFiveHeadersArePresent()
        {
            // Arrange
            var expected = new[]
            {
                "X-Content-Type-Options",
                "X-Frame-Options",
                "Referrer-Policy",
                "Permissions-Policy",
                "X-XSS-Protection"
            };

            // Act
            var ctx = await InvokeAsync();

            // Assert
            foreach (var header in expected)
            {
                ctx.Response.Headers.ContainsKey(header)
                   .Should().BeTrue(because: $"el header {header} debe estar presente");
            }
        }
    }
}
