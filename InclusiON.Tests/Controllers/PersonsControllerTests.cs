using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Tests.Controllers
{
    public class PersonsControllerTests
    {
        private static PersonsController BuildSut(
            IHttpContextService httpCtx,
            IResourceAuthorizationService resourceAuthz)
            => new PersonsController(httpCtx, resourceAuthz);

        private static ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>> OkUpdateLoginMethodHandler()
        {
            var h = Substitute.For<ICommandHandler<UpdateLoginMethodCommand, ApiResponse<UpdateLoginMethodResponse>>>();
            h.HandleAsync(Arg.Any<UpdateLoginMethodCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<UpdateLoginMethodResponse>.SuccessResult(new UpdateLoginMethodResponse()));
            return h;
        }

        // ── UpdateMyLoginMethod ──────────────────────────────────────────────

        [Fact]
        public async Task UpdateMyLoginMethod_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var httpCtx      = Substitute.For<IHttpContextService>();
            var resourceAuthz = Substitute.For<IResourceAuthorizationService>();
            httpCtx.GetCurrentUserId().Returns((Guid?)null);
            var sut     = BuildSut(httpCtx, resourceAuthz);
            var handler = OkUpdateLoginMethodHandler();
            var ct      = CancellationToken.None;

            // Act
            var result = await sut.UpdateMyLoginMethod(
                new UpdateLoginMethodRequest { LoginMethodId = 1 },
                handler,
                ct);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task UpdateMyLoginMethod_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId        = Guid.NewGuid();
            var httpCtx       = Substitute.For<IHttpContextService>();
            var resourceAuthz = Substitute.For<IResourceAuthorizationService>();
            httpCtx.GetCurrentUserId().Returns(userId);
            var sut     = BuildSut(httpCtx, resourceAuthz);
            var handler = OkUpdateLoginMethodHandler();
            var ct      = CancellationToken.None;

            // Act
            await sut.UpdateMyLoginMethod(
                new UpdateLoginMethodRequest { LoginMethodId = 1 },
                handler,
                ct);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<UpdateLoginMethodCommand>(c => c.UserId == userId),
                Arg.Any<CancellationToken>());
        }
    }
}
