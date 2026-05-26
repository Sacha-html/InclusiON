using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Requests.Roadmap;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;
using System.Collections.Generic;

namespace InclusiON.Tests.Controllers
{
    /// <summary>
    /// Verifica que <see cref="RoadmapController"/> requiere un entityId valido en los
    /// endpoints que lo necesitan y que lo propaga correctamente a los handlers.
    /// </summary>
    public class RoadmapControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static RoadmapController BuildSut(Guid? entityId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            return new RoadmapController(httpCtx);
        }

        // ── Handler factories ────────────────────────────────────────────────

        private static IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>> OkGetRoadmapHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>>>();
            handler.HandleAsync(Arg.Any<GetPersonRoadmapQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<RoadmapResponse>.SuccessResult(new RoadmapResponse()));
            return handler;
        }

        private static ICommandHandler<CreateRoadmapCommand, ApiResponse<RoadmapResponse>> OkCreateRoadmapHandler()
        {
            var handler = Substitute.For<ICommandHandler<CreateRoadmapCommand, ApiResponse<RoadmapResponse>>>();
            handler.HandleAsync(Arg.Any<CreateRoadmapCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<RoadmapResponse>.SuccessResult(new RoadmapResponse()));
            return handler;
        }

        private static ICommandHandler<AddRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>> OkAddActivityHandler()
        {
            var handler = Substitute.For<ICommandHandler<AddRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>>>();
            handler.HandleAsync(Arg.Any<AddRoadmapActivityCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<RoadmapActivityResponse>.SuccessResult(new RoadmapActivityResponse()));
            return handler;
        }

        // ── GetMyRoadmap ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyRoadmap_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut = BuildSut(entityId: null);

            // Act
            var result = await sut.GetMyRoadmap(OkGetRoadmapHandler());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyRoadmap_ValidEntityId_PassesPersonIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkGetRoadmapHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.GetMyRoadmap(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetPersonRoadmapQuery>(q => q.PersonId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── CreateRoadmap ─────────────────────────────────────────────────────

        [Fact]
        public async Task CreateRoadmap_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut      = BuildSut(entityId: null);
            var personId = Guid.NewGuid();

            // Act
            var result = await sut.CreateRoadmap(personId, new CreateRoadmapRequest(), OkCreateRoadmapHandler());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateRoadmap_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var handler  = OkCreateRoadmapHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.CreateRoadmap(personId, new CreateRoadmapRequest(), handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateRoadmapCommand>(c => c.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── AddActivity ───────────────────────────────────────────────────────

        [Fact]
        public async Task AddActivity_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut      = BuildSut(entityId: null);
            var personId = Guid.NewGuid();
            var request  = new AddRoadmapActivityRequest { ActivityId = 5 };

            // Act
            var result = await sut.AddActivity(personId, areaId: 1, request, OkAddActivityHandler());

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task AddActivity_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var handler  = OkAddActivityHandler();
            var sut      = BuildSut(entityId: entityId);
            var request  = new AddRoadmapActivityRequest { ActivityId = 5 };

            // Act
            await sut.AddActivity(personId, areaId: 1, request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<AddRoadmapActivityCommand>(c => c.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── GetSkillRadar (IN-90) ─────────────────────────────────────────────

        [Fact]
        public async Task GetSkillRadar_PassesPersonIdToHandler()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var handler  = Substitute.For<IQueryHandler<GetSkillRadarQuery, ApiResponse<List<SkillRadarPointResponse>>>>();
            handler.HandleAsync(Arg.Any<GetSkillRadarQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<List<SkillRadarPointResponse>>.SuccessResult([]));
            var sut = BuildSut(entityId: Guid.NewGuid());

            // Act
            await sut.GetSkillRadar(personId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetSkillRadarQuery>(q => q.PersonId == personId),
                Arg.Any<CancellationToken>());
        }

        // ── GetAdaptiveConfig (IN-116) ────────────────────────────────────────

        [Fact]
        public async Task GetAdaptiveConfig_PassesActivityEntryIdToHandler()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var handler  = Substitute.For<IQueryHandler<GetAdaptiveEngineConfigQuery, ApiResponse<AdaptiveEngineConfigResponse?>>>();
            handler.HandleAsync(Arg.Any<GetAdaptiveEngineConfigQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<AdaptiveEngineConfigResponse?>.SuccessResult(new AdaptiveEngineConfigResponse()));
            var sut = BuildSut(entityId: Guid.NewGuid());

            // Act
            await sut.GetAdaptiveConfig(personId, areaId: 2, activityEntryId: 99, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetAdaptiveEngineConfigQuery>(q => q.PersonRoadmapActivityId == 99),
                Arg.Any<CancellationToken>());
        }

        // ── UpsertAdaptiveConfig (IN-116) ─────────────────────────────────────

        [Fact]
        public async Task UpsertAdaptiveConfig_PassesAllFieldsToCommand()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var request  = new UpsertAdaptiveEngineConfigRequest
            {
                IsEnabled                      = true,
                MinDifficultyLevel             = 1,
                MaxDifficultyLevel             = 5,
                ConsecutiveSuccessToUpgrade    = 4,
                ConsecutiveFailuresToDowngrade = 3,
                SuccessThresholdPercent        = 75,
                FrustrationThreshold           = 4,
            };
            var handler = Substitute.For<ICommandHandler<UpsertAdaptiveEngineConfigCommand, ApiResponse<AdaptiveEngineConfigResponse>>>();
            handler.HandleAsync(Arg.Any<UpsertAdaptiveEngineConfigCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<AdaptiveEngineConfigResponse>.SuccessResult(new AdaptiveEngineConfigResponse()));
            var sut = BuildSut(entityId: Guid.NewGuid());

            // Act
            await sut.UpsertAdaptiveConfig(personId, areaId: 1, activityEntryId: 7, request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<UpsertAdaptiveEngineConfigCommand>(c =>
                    c.PersonRoadmapActivityId        == 7   &&
                    c.SuccessThresholdPercent        == 75  &&
                    c.FrustrationThreshold           == 4   &&
                    c.ConsecutiveSuccessToUpgrade    == 4),
                Arg.Any<CancellationToken>());
        }

        // ── DeleteAdaptiveConfig (IN-116) ─────────────────────────────────────

        [Fact]
        public async Task DeleteAdaptiveConfig_PassesActivityEntryIdToCommand()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var handler  = Substitute.For<ICommandHandler<DeleteAdaptiveEngineConfigCommand, ApiResponse<object>>>();
            handler.HandleAsync(Arg.Any<DeleteAdaptiveEngineConfigCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<object>.SuccessResult(null!));
            var sut = BuildSut(entityId: Guid.NewGuid());

            // Act
            await sut.DeleteAdaptiveConfig(personId, areaId: 2, activityEntryId: 55, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<DeleteAdaptiveEngineConfigCommand>(c => c.PersonRoadmapActivityId == 55),
                Arg.Any<CancellationToken>());
        }
    }
}
