using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Users.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Controllers
{
    public class UsersControllerTests
    {
        private static UsersController BuildSut(IHttpContextService httpCtx)
            => new UsersController(httpCtx);

        private static IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>> OkGetProfileHandler()
        {
            var h = Substitute.For<IQueryHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>>();
            h.HandleAsync(Arg.Any<GetUserProfileQuery>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<UserProfileResponse>.SuccessResult(new UserProfileResponse()));
            return h;
        }

        // ── GetMyProfile ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyProfile_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx);
            var handler = OkGetProfileHandler();

            // Act
            var result = await sut.GetMyProfile(handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetMyProfile_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId  = Guid.NewGuid();
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns(userId);
            var sut     = BuildSut(httpCtx);
            var handler = OkGetProfileHandler();

            // Act
            await sut.GetMyProfile(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetUserProfileQuery>(q => q.UserId == userId),
                Arg.Any<CancellationToken>());
        }
    }
}
