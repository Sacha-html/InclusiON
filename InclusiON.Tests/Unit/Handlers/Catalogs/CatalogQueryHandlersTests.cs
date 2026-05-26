using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Handlers;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Catalogs
{
    public class CatalogQueryHandlersTests
    {
        private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

        // ── DisabilityTypes ──────────────────────────────────────────────

        [Fact]
        public async Task GetDisabilityTypes_ReturnsAll()
        {
            var repo = Substitute.For<IReadOnlyRepository<DisabilityType>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<DisabilityType>
                {
                    new() { Id = 1, Name = "Motriz" },
                    new() { Id = 2, Name = "Sensorial" }
                });

            var handler = new GetDisabilityTypesQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>());
            var result = await handler.HandleAsync(new GetDisabilityTypesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].Name.Should().Be("Motriz");
        }

        [Fact]
        public async Task GetDisabilityTypes_Empty_ReturnsEmptyList()
        {
            var repo = Substitute.For<IReadOnlyRepository<DisabilityType>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<DisabilityType>());

            var result = await new GetDisabilityTypesQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>())
                .HandleAsync(new GetDisabilityTypesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        // ── AutonomyLevels ───────────────────────────────────────────────

        [Fact]
        public async Task GetAutonomyLevels_ReturnsAll()
        {
            var repo = Substitute.For<IReadOnlyRepository<AutonomyLevel>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<AutonomyLevel>
                {
                    new() { Id = 1, Name = "Alta", DisplayOrder = 1 },
                    new() { Id = 2, Name = "Media", DisplayOrder = 2 }
                });

            var result = await new GetAutonomyLevelsQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>())
                .HandleAsync(new GetAutonomyLevelsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data![0].Name.Should().Be("Alta");
        }

        // ── SkillAreas ───────────────────────────────────────────────────

        [Fact]
        public async Task GetSkillAreas_ReturnsAll()
        {
            var repo = Substitute.For<IReadOnlyRepository<SkillArea>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<SkillArea>
                {
                    new() { Id = 1, Name = "Comunicación" },
                    new() { Id = 2, Name = "Autonomía" }
                });

            var result = await new GetSkillAreasQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>())
                .HandleAsync(new GetSkillAreasQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        // ── ActivityCategories ───────────────────────────────────────────

        [Fact]
        public async Task GetActivityCategories_ReturnsAll()
        {
            var repo = Substitute.For<IReadOnlyRepository<ActivityCategory>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<ActivityCategory>
                {
                    new() { Id = 1, Name = "Cognitiva" }
                });

            var result = await new GetActivityCategoriesQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>())
                .HandleAsync(new GetActivityCategoriesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].Name.Should().Be("Cognitiva");
        }

        // ── ActivityTemplateTypes ────────────────────────────────────────

        [Fact]
        public async Task GetActivityTemplateTypes_ReturnsAll()
        {
            var repo = Substitute.For<IReadOnlyRepository<ActivityTemplateType>>();
            repo.GetAllActiveAsync(Arg.Any<CancellationToken>())
                .Returns(new List<ActivityTemplateType>
                {
                    new() { Id = 1, Name = "Selección", Code = "SELECTION", ContentSchema = "{}", ComponentName = "SelectionComponent" }
                });

            var result = await new GetActivityTemplateTypesQueryHandler(repo, CreateCache(), Substitute.For<IEncryptionService>())
                .HandleAsync(new GetActivityTemplateTypesQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].Code.Should().Be("SELECTION");
        }

        // ── AvatarColors ─────────────────────────────────────────────────

        [Fact]
        public async Task GetAvatarColors_ReturnsStaticList()
        {
            var result = await new GetAvatarColorsQueryHandler()
                .HandleAsync(new GetAvatarColorsQuery(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
            result.Data!.All(c => !string.IsNullOrWhiteSpace(c.Hex)).Should().BeTrue();
        }
    }
}
