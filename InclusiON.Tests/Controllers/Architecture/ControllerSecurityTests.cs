using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using InclusiON.Api.Controllers;

namespace InclusiON.Tests.Controllers.Architecture
{
    /// <summary>
    /// Verifica que los controllers apliquen correctamente los atributos de seguridad
    /// y no expongan endpoints desprotegidos involuntariamente.
    /// </summary>
    public class ControllerSecurityTests
    {
        private static readonly Assembly ApiAssembly = typeof(DiagnosesController).Assembly;

        // ── Todos los controllers heredan de ControllerBase ───────────────────

        [Fact]
        public void Controllers_ShouldInherit_ControllerBase()
        {
            var result = Types.InAssembly(ApiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(ControllerBase))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "todo controller debe heredar de ControllerBase — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Todo controller tiene [ApiController] ─────────────────────────────

        [Fact]
        public void Controllers_ShouldHave_ApiControllerAttribute()
        {
            var result = Types.InAssembly(ApiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .And()
                .AreNotAbstract()
                .Should()
                .HaveCustomAttribute(typeof(ApiControllerAttribute))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "todo controller debe tener [ApiController] para activar validación automática de modelos — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Todo controller tiene [Route] ─────────────────────────────────────

        [Fact]
        public void Controllers_ShouldHave_RouteAttribute()
        {
            var result = Types.InAssembly(ApiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .And()
                .AreNotAbstract()
                .Should()
                .HaveCustomAttribute(typeof(RouteAttribute))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "todo controller debe tener [Route] explícito — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Todo controller protegido tiene cobertura de autorización ─────────

        [Fact]
        public void Controllers_ShouldHave_AuthorizationCoverage()
        {
            // AuthController y HealthUiController tienen endpoints públicos por diseño:
            // AuthController → Login, Register, etc. son anónimos por definición
            // HealthUiController → health checks son públicos intencionalmente
            var publicByDesign = new HashSet<string> { "AuthController", "HealthUiController" };

            // Un controller protegido está cubierto si:
            // a) tiene [Authorize] a nivel de clase, O
            // b) todos sus action methods tienen [Authorize] o [AllowAnonymous]
            var controllers = ApiAssembly.GetTypes()
                .Where(t => t.IsClass
                         && !t.IsAbstract
                         && t.Name.EndsWith("Controller")
                         && t.IsAssignableTo(typeof(ControllerBase))
                         && !publicByDesign.Contains(t.Name))
                .ToList();

            var unprotected = new List<string>();

            foreach (var controller in controllers)
            {
                bool classHasAuthorize = controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any();
                bool classHasAllowAnon = controller.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any();

                if (classHasAuthorize || classHasAllowAnon) continue;

                // Sin [Authorize] de clase: verificar que cada action esté cubierta
                var actions = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.GetCustomAttributes<HttpGetAttribute>().Any()
                             || m.GetCustomAttributes<HttpPostAttribute>().Any()
                             || m.GetCustomAttributes<HttpPutAttribute>().Any()
                             || m.GetCustomAttributes<HttpPatchAttribute>().Any()
                             || m.GetCustomAttributes<HttpDeleteAttribute>().Any());

                var uncovered = actions
                    .Where(m => !m.GetCustomAttributes<AuthorizeAttribute>().Any()
                             && !m.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                    .Select(m => $"{controller.Name}.{m.Name}")
                    .ToList();

                unprotected.AddRange(uncovered);
            }

            unprotected.Should().BeEmpty(
                because: "todo action method debe tener [Authorize] o [AllowAnonymous] explícito — " +
                         "métodos sin cobertura: {0}", string.Join(", ", unprotected));
        }

        // ── HealthUiController puede ser excepción ────────────────────────────

        [Fact]
        public void Controllers_ShouldResideIn_Api_Namespace()
        {
            var result = Types.InAssembly(ApiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .And()
                .AreNotAbstract()
                .Should()
                .ResideInNamespace("InclusiON.Api.Controllers")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "todos los controllers deben estar en InclusiON.Api.Controllers — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static string FailingTypes(TestResult result) =>
            result.FailingTypes is null || result.FailingTypes.Count == 0
                ? "(ninguno)"
                : string.Join(", ", result.FailingTypes.Select(t => t.Name));
    }
}
