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
    }
}
