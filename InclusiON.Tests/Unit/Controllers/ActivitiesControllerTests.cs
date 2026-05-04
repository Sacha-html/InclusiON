using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Activities;
using InclusiON.DTOs.Requests.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Tests.Unit.Controllers
{
    public class ActivitiesControllerTests
    {
        private static ActivitiesController BuildSut(Guid? entityId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            return new ActivitiesController(httpCtx);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static IQueryHandler<SearchActivitiesSemanticQuery, ApiResponse<List<ActivityListItemResponse>>> OkSemanticHandler()
        {
            var h = Substitute.For<IQueryHandler<SearchActivitiesSemanticQuery, ApiResponse<List<ActivityListItemResponse>>>>();
            h.HandleAsync(Arg.Any<SearchActivitiesSemanticQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<List<ActivityListItemResponse>>.SuccessResult(new List<ActivityListItemResponse>()));
            return h;
        }

        private static IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>> OkGetActivitiesHandler()
        {
            var h = Substitute.For<IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>>>();
            h.HandleAsync(Arg.Any<GetActivitiesQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<PagedResponse<ActivityListItemResponse>>.SuccessResult(new PagedResponse<ActivityListItemResponse>()));
            return h;
        }

        private static IQueryHandler<GetActivityByIdQuery, ApiResponse<ActivityResponse>> OkGetActivityHandler()
        {
            var h = Substitute.For<IQueryHandler<GetActivityByIdQuery, ApiResponse<ActivityResponse>>>();
            h.HandleAsync(Arg.Any<GetActivityByIdQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityResponse>.SuccessResult(new ActivityResponse()));
            return h;
        }

        private static ICommandHandler<CreateActivityCommand, ApiResponse<ActivityResponse>> OkCreateActivityHandler()
        {
            var h = Substitute.For<ICommandHandler<CreateActivityCommand, ApiResponse<ActivityResponse>>>();
            h.HandleAsync(Arg.Any<CreateActivityCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityResponse>.SuccessResult(new ActivityResponse { Id = 1, Title = "Test" }));
            return h;
        }

        private static ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>> OkUpdateActivityHandler()
        {
            var h = Substitute.For<ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>>>();
            h.HandleAsync(Arg.Any<UpdateActivityCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityResponse>.SuccessResult(new ActivityResponse()));
            return h;
        }

        private static ICommandHandler<PatchActivityStatusCommand, ApiResponse<ActivityResponse>> OkPatchStatusHandler()
        {
            var h = Substitute.For<ICommandHandler<PatchActivityStatusCommand, ApiResponse<ActivityResponse>>>();
            h.HandleAsync(Arg.Any<PatchActivityStatusCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityResponse>.SuccessResult(new ActivityResponse()));
            return h;
        }

        // ── SearchActivitiesSemantic ─────────────────────────────────────────

        [Fact]
        public async Task SearchActivitiesSemantic_EmptyText_ReturnsBadRequest()
        {
            // Arrange
            var sut     = BuildSut(entityId: Guid.NewGuid());
            var handler = OkSemanticHandler();

            // Act
            var result = await sut.SearchActivitiesSemantic("   ", handler, limit: 10);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SearchActivitiesSemantic_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkSemanticHandler();

            // Act
            var result = await sut.SearchActivitiesSemantic("test", handler, limit: 10);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SearchActivitiesSemantic_ValidParams_PassesProfessionalIdAndTextToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkSemanticHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.SearchActivitiesSemantic("animals", handler, limit: 10);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<SearchActivitiesSemanticQuery>(q =>
                    q.ProfessionalId == entityId && q.Text == "animals"),
                Arg.Any<CancellationToken>());
        }

        // ── GetActivities ────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivities_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkGetActivitiesHandler();

            // Act
            var result = await sut.GetActivities(new GetActivitiesRequest(), handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetActivities_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkGetActivitiesHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.GetActivities(new GetActivitiesRequest(), handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetActivitiesQuery>(q => q.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── GetActivity ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetActivity_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkGetActivityHandler();

            // Act
            var result = await sut.GetActivity(5, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetActivity_ValidEntityId_PassesProfessionalIdAndIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkGetActivityHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.GetActivity(5, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetActivityByIdQuery>(q =>
                    q.ActivityId == 5 && q.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── CreateActivity ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateActivity_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkCreateActivityHandler();
            var request = new CreateActivityRequest
            {
                Title          = "Test",
                CategoryId     = 1,
                TemplateTypeId = 1,
                ContentJson    = "{}"
            };

            // Act
            var result = await sut.CreateActivity(request, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateActivity_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkCreateActivityHandler();
            var sut      = BuildSut(entityId: entityId);
            var request  = new CreateActivityRequest
            {
                Title          = "Test",
                CategoryId     = 1,
                TemplateTypeId = 1,
                ContentJson    = "{}"
            };

            // Act
            await sut.CreateActivity(request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateActivityCommand>(c => c.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── UpdateActivity ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateActivity_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkUpdateActivityHandler();
            var request = new UpdateActivityRequest
            {
                Title       = "Test",
                CategoryId  = 1,
                ContentJson = "{}"
            };

            // Act
            var result = await sut.UpdateActivity(7, request, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateActivity_ValidEntityId_PassesProfessionalIdAndIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkUpdateActivityHandler();
            var sut      = BuildSut(entityId: entityId);
            var request  = new UpdateActivityRequest
            {
                Title       = "Test",
                CategoryId  = 1,
                ContentJson = "{}"
            };

            // Act
            await sut.UpdateActivity(7, request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<UpdateActivityCommand>(c =>
                    c.ProfessionalId == entityId && c.ActivityId == 7),
                Arg.Any<CancellationToken>());
        }

        // ── PatchActivityStatus ──────────────────────────────────────────────

        [Fact]
        public async Task PatchActivityStatus_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkPatchStatusHandler();
            var request = new PatchStatusRequest(false);

            // Act
            var result = await sut.PatchActivityStatus(3, request, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task PatchActivityStatus_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkPatchStatusHandler();
            var sut      = BuildSut(entityId: entityId);
            var request  = new PatchStatusRequest(false);

            // Act
            await sut.PatchActivityStatus(3, request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<PatchActivityStatusCommand>(c => c.ProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }
    }
}
