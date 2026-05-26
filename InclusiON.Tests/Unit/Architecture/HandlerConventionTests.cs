using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using InclusiON.Application.Interfaces.Common;

namespace InclusiON.Tests.Unit.Architecture
{
    /// <summary>
    /// Verifica que todos los handlers del sistema respeten las convenciones de nombre
    /// e implementen las interfaces correctas del patrón CQRS.
    /// </summary>
    public class HandlerConventionTests
    {
        private static readonly System.Reflection.Assembly ApplicationAssembly =
            typeof(ICommandHandler<,>).Assembly;

        // ── CommandHandlers ───────────────────────────────────────────────────

        [Fact]
        public void ClassesEndingWith_CommandHandler_ShouldImplement_ICommandHandler()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("CommandHandler")
                .Should()
                .ImplementInterface(typeof(ICommandHandler<,>))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "toda clase con sufijo 'CommandHandler' debe implementar ICommandHandler<TCommand, TResult> — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void ClassesImplementing_ICommandHandler_ShouldEndWith_CommandHandler()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(ICommandHandler<,>))
                .Should()
                .HaveNameEndingWith("CommandHandler")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "toda clase que implemente ICommandHandler debe terminar en 'CommandHandler' — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── QueryHandlers ─────────────────────────────────────────────────────

        [Fact]
        public void ClassesEndingWith_QueryHandler_ShouldImplement_IQueryHandler()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("QueryHandler")
                .Should()
                .ImplementInterface(typeof(IQueryHandler<,>))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "toda clase con sufijo 'QueryHandler' debe implementar IQueryHandler<TQuery, TResult> — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void ClassesImplementing_IQueryHandler_ShouldEndWith_QueryHandler()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(IQueryHandler<,>))
                .Should()
                .HaveNameEndingWith("QueryHandler")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "toda clase que implemente IQueryHandler debe terminar en 'QueryHandler' — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Ubicación ──────────────────────────────────────────────────────────

        [Fact]
        public void CommandHandlers_ShouldResideIn_Application_Namespace()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(ICommandHandler<,>))
                .Should()
                .ResideInNamespace("InclusiON.Application")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "los command handlers deben estar dentro de InclusiON.Application — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        [Fact]
        public void QueryHandlers_ShouldResideIn_Application_Namespace()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(IQueryHandler<,>))
                .Should()
                .ResideInNamespace("InclusiON.Application")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: "los query handlers deben estar dentro de InclusiON.Application — " +
                         "los tipos fallidos son: {0}", FailingTypes(result));
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static string FailingTypes(TestResult result) =>
            result.FailingTypes is null || result.FailingTypes.Count == 0
                ? "(ninguno)"
                : string.Join(", ", result.FailingTypes.Select(t => t.Name));
    }
}
