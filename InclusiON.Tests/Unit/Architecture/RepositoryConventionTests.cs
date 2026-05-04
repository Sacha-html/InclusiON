using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Infrastructure.Data.Repositories;

namespace InclusiON.Tests.Unit.Architecture
{
    /// <summary>
    /// Verifica que las implementaciones de repositorios estén en la capa correcta
    /// y que las interfaces de repositorios estén definidas en Application.
    /// </summary>
    public class RepositoryConventionTests
    {
        private static readonly System.Reflection.Assembly ApplicationAssembly    = typeof(IReportsRepository).Assembly;
        private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(DiagnosesRepository).Assembly;

        // ── Interfaces en Application ─────────────────────────────────────────

        [Fact]
        public void RepositoryInterfaces_ShouldResideIn_Application_Namespace()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("Repository")
                .And()
                .AreInterfaces()
                .Should()
                .ResideInNamespace("InclusiON.Application.Interfaces.Repositories")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "las interfaces de repositorios deben estar en Application.Interfaces.Repositories — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Implementaciones en Infrastructure ────────────────────────────────

        [Fact]
        public void RepositoryImplementations_ShouldResideIn_Infrastructure_Namespace()
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("Repository")
                .And()
                .AreNotInterfaces()
                .Should()
                .ResideInNamespace("InclusiON.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "las implementaciones de repositorios deben estar en InclusiON.Infrastructure — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void RepositoryImplementations_ShouldImplement_RepositoryInterface()
        {
            // Toda clase concreta llamada XxxRepository debe implementar al menos una interfaz IXxxRepository
            var repoTypes = Types.InAssembly(InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("Repository")
                .And()
                .AreNotInterfaces()
                .GetTypes();

            var withoutInterface = repoTypes
                .Where(t => !t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")))
                .Select(t => t.Name)
                .ToList();

            withoutInterface.Should().BeEmpty(
                because: "toda clase XxxRepository debe implementar IXxxRepository — " +
                         "los tipos fallidos son: {0}", string.Join(", ", withoutInterface));
        }

        // ── Application no tiene implementaciones ─────────────────────────────

        [Fact]
        public void Application_ShouldNot_ContainConcreteRepositories()
        {
            var concreteRepos = Types.InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("Repository")
                .And()
                .AreNotInterfaces()
                .GetTypes()
                .Select(t => t.Name)
                .ToList();

            concreteRepos.Should().BeEmpty(
                because: "Application no debe contener implementaciones concretas de repositorios, solo interfaces — " +
                         "los tipos encontrados son: {0}", string.Join(", ", concreteRepos));
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static string FailingTypes(TestResult result) =>
            result.FailingTypes is null || result.FailingTypes.Count == 0
                ? "(ninguno)"
                : string.Join(", ", result.FailingTypes.Select(t => t.Name));
    }
}
