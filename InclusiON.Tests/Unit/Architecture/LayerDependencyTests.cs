using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using InclusiON.Domain.Models;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Infrastructure.Data.Repositories;
using InclusiON.Api.Controllers;

namespace InclusiON.Tests.Unit.Architecture
{
    /// <summary>
    /// Verifica que las dependencias entre capas respeten la arquitectura limpia:
    /// Domain → sin dependencias externas
    /// Application → solo puede referenciar Domain y DTOs
    /// Infrastructure → puede referenciar Application y Domain
    /// Api → puede referenciar Application, no Infrastructure directamente
    /// </summary>
    public class LayerDependencyTests
    {
        private static readonly System.Reflection.Assembly DomainAssembly         = typeof(Report).Assembly;
        private static readonly System.Reflection.Assembly ApplicationAssembly    = typeof(ICommandHandler<,>).Assembly;
        private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(DiagnosesRepository).Assembly;
        private static readonly System.Reflection.Assembly ApiAssembly            = typeof(DiagnosesController).Assembly;

        // ── Domain ───────────────────────────────────────────────────────────

        [Fact]
        public void Domain_ShouldNot_DependOn_Application()
        {
            var result = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Application")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "el dominio no debe depender de la capa de aplicación — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void Domain_ShouldNot_DependOn_Infrastructure()
        {
            var result = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "el dominio no debe depender de infraestructura — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void Domain_ShouldNot_DependOn_Api()
        {
            var result = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Api")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "el dominio no debe depender de la API — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void Domain_ShouldNot_DependOn_Data()
        {
            var result = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Data")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "el dominio no debe depender de la capa de datos — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Application ──────────────────────────────────────────────────────

        [Fact]
        public void Application_ShouldNot_DependOn_Infrastructure()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "la capa de aplicación no debe conocer infraestructura — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void Application_ShouldNot_DependOn_Api()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Api")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "la capa de aplicación no debe conocer la API — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void Application_ShouldNot_DependOn_Data()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Data")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "la capa de aplicación no debe depender directamente de AppDbContext — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Infrastructure ───────────────────────────────────────────────────

        [Fact]
        public void Infrastructure_ShouldNot_DependOn_Api()
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Api")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "infraestructura no debe conocer la capa de API — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Api ───────────────────────────────────────────────────────────────

        [Fact]
        public void Api_ShouldNot_DependOn_Infrastructure_Directly()
        {
            var result = Types.InAssembly(ApiAssembly)
                .That()
                .ResideInNamespace("InclusiON.Api.Controllers")
                .ShouldNot()
                .HaveDependencyOn("InclusiON.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "los controllers no deben depender directamente de infraestructura — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Helper ───────────────────────────────────────────────────────────

        private static string FailingTypes(TestResult result) =>
            result.FailingTypes is null || result.FailingTypes.Count == 0
                ? "(ninguno)"
                : string.Join(", ", result.FailingTypes.Select(t => t.Name));
    }
}
