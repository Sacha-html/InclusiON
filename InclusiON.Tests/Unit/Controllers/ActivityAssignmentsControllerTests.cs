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
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Tests.Unit.Controllers
{
    public class ActivityAssignmentsControllerTests
    {
        private static ActivityAssignmentsController BuildSut(Guid? entityId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentEntityId().Returns(entityId);
            return new ActivityAssignmentsController(httpCtx);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static IQueryHandler<GetAssignmentByIdQuery, ApiResponse<ActivityAssignmentResponse>> OkGetAssignmentByIdHandler()
        {
            var h = Substitute.For<IQueryHandler<GetAssignmentByIdQuery, ApiResponse<ActivityAssignmentResponse>>>();
            h.HandleAsync(Arg.Any<GetAssignmentByIdQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityAssignmentResponse>.SuccessResult(new ActivityAssignmentResponse()));
            return h;
        }

        private static ICommandHandler<CreateActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>> OkCreateAssignmentHandler()
        {
            var h = Substitute.For<ICommandHandler<CreateActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>>>();
            h.HandleAsync(Arg.Any<CreateActivityAssignmentCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityAssignmentResponse>.SuccessResult(new ActivityAssignmentResponse()));
            return h;
        }

        private static IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>> OkGetPersonAssignmentsHandler()
        {
            var h = Substitute.For<IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>>>();
            h.HandleAsync(Arg.Any<GetPersonActivityAssignmentsQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<List<ActivityAssignmentResponse>>.SuccessResult(new List<ActivityAssignmentResponse>()));
            return h;
        }

        private static ICommandHandler<StartActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>> OkStartResponseHandler()
        {
            var h = Substitute.For<ICommandHandler<StartActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>>>();
            h.HandleAsync(Arg.Any<StartActivityResponseCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityAssignmentResponse>.SuccessResult(new ActivityAssignmentResponse()));
            return h;
        }

        private static ICommandHandler<CompleteActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>> OkCompleteResponseHandler()
        {
            var h = Substitute.For<ICommandHandler<CompleteActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>>>();
            h.HandleAsync(Arg.Any<CompleteActivityResponseCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityAssignmentResponse>.SuccessResult(new ActivityAssignmentResponse()));
            return h;
        }

        private static ICommandHandler<CancelActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>> OkCancelAssignmentHandler()
        {
            var h = Substitute.For<ICommandHandler<CancelActivityAssignmentCommand, ApiResponse<ActivityAssignmentResponse>>>();
            h.HandleAsync(Arg.Any<CancelActivityAssignmentCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ActivityAssignmentResponse>.SuccessResult(new ActivityAssignmentResponse()));
            return h;
        }

        // ── GetAssignmentById ────────────────────────────────────────────────

        [Fact]
        public async Task GetAssignmentById_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkGetAssignmentByIdHandler();

            // Act
            var result = await sut.GetAssignmentById(1, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAssignmentById_ValidEntityId_PassesRequesterIdToHandler()
        {
            // Arrange
            var entityId     = Guid.NewGuid();
            var assignmentId = 42;
            var handler      = OkGetAssignmentByIdHandler();
            var sut          = BuildSut(entityId: entityId);

            // Act
            await sut.GetAssignmentById(assignmentId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetAssignmentByIdQuery>(q =>
                    q.AssignmentId == assignmentId && q.RequesterId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── CreateAssignment ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateAssignment_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkCreateAssignmentHandler();
            var request = new CreateActivityAssignmentRequest { EncryptedActivityId = "ENCRYPTED_1", PersonId = Guid.NewGuid() };

            // Act
            var result = await sut.CreateAssignment(request, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateAssignment_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkCreateAssignmentHandler();
            var sut      = BuildSut(entityId: entityId);
            var request  = new CreateActivityAssignmentRequest { EncryptedActivityId = "ENCRYPTED_1", PersonId = Guid.NewGuid() };

            // Act
            await sut.CreateAssignment(request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CreateActivityAssignmentCommand>(c => c.AssignedByProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── GetPersonAssignments ─────────────────────────────────────────────

        [Fact]
        public async Task GetPersonAssignments_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkGetPersonAssignmentsHandler();

            // Act
            var result = await sut.GetPersonAssignments(Guid.NewGuid(), handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPersonAssignments_ValidEntityId_PassesPersonIdAndRequesterIdToHandler()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var handler  = OkGetPersonAssignmentsHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.GetPersonAssignments(personId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetPersonActivityAssignmentsQuery>(q =>
                    q.PersonId == personId && q.RequesterId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── GetMyAssignments ─────────────────────────────────────────────────

        [Fact]
        public async Task GetMyAssignments_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkGetPersonAssignmentsHandler();

            // Act
            var result = await sut.GetMyAssignments(handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetMyAssignments_ValidEntityId_UsesEntityIdAsBothPersonIdAndRequesterId()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var handler  = OkGetPersonAssignmentsHandler();
            var sut      = BuildSut(entityId: entityId);

            // Act
            await sut.GetMyAssignments(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetPersonActivityAssignmentsQuery>(q =>
                    q.PersonId == entityId && q.RequesterId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── StartResponse ────────────────────────────────────────────────────

        [Fact]
        public async Task StartResponse_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkStartResponseHandler();

            // Act
            var result = await sut.StartResponse(1, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task StartResponse_ValidEntityId_PassesPersonIdToHandler()
        {
            // Arrange
            var entityId     = Guid.NewGuid();
            var assignmentId = 10;
            var handler      = OkStartResponseHandler();
            var sut          = BuildSut(entityId: entityId);

            // Act
            await sut.StartResponse(assignmentId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<StartActivityResponseCommand>(c => c.PersonId == entityId),
                Arg.Any<CancellationToken>());
        }

        // ── CompleteResponse ─────────────────────────────────────────────────

        [Fact]
        public async Task CompleteResponse_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkCompleteResponseHandler();
            var request = new CompleteActivityResponseRequest { SuccessPercentage = 100, TimeSpentSeconds = 60 };

            // Act
            var result = await sut.CompleteResponse(1, 1, request, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ── CancelAssignment ─────────────────────────────────────────────────

        [Fact]
        public async Task CancelAssignment_NullEntityId_ReturnsNotFound()
        {
            // Arrange
            var sut     = BuildSut(entityId: null);
            var handler = OkCancelAssignmentHandler();

            // Act
            var result = await sut.CancelAssignment(5, handler);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CancelAssignment_ValidEntityId_PassesProfessionalIdToHandler()
        {
            // Arrange
            var entityId     = Guid.NewGuid();
            var assignmentId = 5;
            var handler      = OkCancelAssignmentHandler();
            var sut          = BuildSut(entityId: entityId);

            // Act
            await sut.CancelAssignment(assignmentId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<CancelActivityAssignmentCommand>(c => c.RequestedByProfessionalId == entityId),
                Arg.Any<CancellationToken>());
        }
    }
}
