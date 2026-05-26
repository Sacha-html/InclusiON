using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Family;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Tests.Controllers
{
    public class FamilyControllerTests
    {
        private static FamilyController BuildSut(IHttpContextService httpCtx)
            => new FamilyController(httpCtx);

        private static IQueryHandler<GetFamilyDashboardQuery, ApiResponse<FamilyDashboardResponse>> OkDashboardHandler()
        {
            var h = Substitute.For<IQueryHandler<GetFamilyDashboardQuery, ApiResponse<FamilyDashboardResponse>>>();
            h.HandleAsync(Arg.Any<GetFamilyDashboardQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<FamilyDashboardResponse>.SuccessResult(new FamilyDashboardResponse()));
            return h;
        }

        // ── GetDashboard ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetDashboard_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = OkDashboardHandler();

            // Act
            var result = await sut.GetDashboard(handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetDashboard_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId  = Guid.NewGuid();
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns(userId);
            var sut     = BuildSut(httpCtx);
            var handler = OkDashboardHandler();

            // Act
            await sut.GetDashboard(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetFamilyDashboardQuery>(q => q.FamilyUserId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── LinkFamilyToPerson ────────────────────────────────────────────────

        [Fact]
        public async Task LinkFamilyToPerson_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = Substitute.For<ICommandHandler<LinkFamilyToPersonCommand, ApiResponse<PersonRepresentativeResponse>>>();
            var request = new LinkFamilyToPersonRequest { Relationship = "Parent" };

            // Act
            var result = await sut.LinkFamilyToPerson(Guid.NewGuid(), Guid.NewGuid(), request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── UnlinkFamilyFromPerson ────────────────────────────────────────────

        [Fact]
        public async Task UnlinkFamilyFromPerson_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = Substitute.For<ICommandHandler<UnlinkFamilyFromPersonCommand, ApiResponse<PersonRepresentativeResponse>>>();
            var request = new UnlinkFamilyFromPersonRequest { Observation = "reason" };

            // Act
            var result = await sut.UnlinkFamilyFromPerson(Guid.NewGuid(), Guid.NewGuid(), request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── LinkFamilyToPersonAsProfessional ──────────────────────────────────

        [Fact]
        public async Task LinkFamilyToPersonAsProfessional_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = Substitute.For<ICommandHandler<LinkFamilyToPersonCommand, ApiResponse<PersonRepresentativeResponse>>>();
            var request = new LinkFamilyToPersonRequest { Relationship = "Parent" };

            // Act
            var result = await sut.LinkFamilyToPersonAsProfessional(Guid.NewGuid(), Guid.NewGuid(), request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── UnlinkFamilyFromPersonAsProfessional ──────────────────────────────

        [Fact]
        public async Task UnlinkFamilyFromPersonAsProfessional_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = Substitute.For<ICommandHandler<UnlinkFamilyFromPersonCommand, ApiResponse<PersonRepresentativeResponse>>>();
            var request = new UnlinkFamilyFromPersonRequest { Observation = "reason" };

            // Act
            var result = await sut.UnlinkFamilyFromPersonAsProfessional(Guid.NewGuid(), Guid.NewGuid(), request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
