using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Auth;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Tests.Controllers
{
    /// <summary>
    /// Verifica el comportamiento de <see cref="AuthController"/> en los casos
    /// donde se requiere autenticacion o el handler devuelve un resultado de fallo.
    /// </summary>
    public class AuthControllerTests
    {
        // ── Handler factories ────────────────────────────────────────────────

        private static ICommandHandler<ChangePasswordCommand, ApiResponse<ChangePasswordResponse>> OkChangePasswordHandler()
        {
            var handler = Substitute.For<ICommandHandler<ChangePasswordCommand, ApiResponse<ChangePasswordResponse>>>();
            handler.HandleAsync(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<ChangePasswordResponse>.SuccessResult(new ChangePasswordResponse()));
            return handler;
        }

        private static ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>> OkVisualStandardHandler()
        {
            var handler = Substitute.For<ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>>>();
            handler.HandleAsync(Arg.Any<VisualStandardLoginCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<VisualLoginResponse>.SuccessResult(new VisualLoginResponse { Success = true }));
            return handler;
        }

        private static ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>> OkPinLoginHandler()
        {
            var handler = Substitute.For<ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>>>();
            handler.HandleAsync(Arg.Any<PinLoginCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<VisualLoginResponse>.SuccessResult(new VisualLoginResponse { Success = true }));
            return handler;
        }

        // ── ChangePassword ───────────────────────────────────────────────────

        [Fact]
        public async Task ChangePassword_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut            = new AuthController();
            var httpCtxService = Substitute.For<IHttpContextService>();
            httpCtxService.GetCurrentUserId().Returns((Guid?)null);
            var request = new ChangePasswordRequest
            {
                CurrentPassword  = "old",
                NewPassword      = "New1!",
                ConfirmNewPassword = "New1!"
            };

            // Act
            var result = await sut.ChangePassword(request, OkChangePasswordHandler(), httpCtxService);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId         = Guid.NewGuid();
            var sut            = new AuthController();
            var httpCtxService = Substitute.For<IHttpContextService>();
            httpCtxService.GetCurrentUserId().Returns(userId);
            var handler = OkChangePasswordHandler();
            var request = new ChangePasswordRequest
            {
                CurrentPassword    = "old",
                NewPassword        = "New1!",
                ConfirmNewPassword = "New1!"
            };

            // Act
            await sut.ChangePassword(request, handler, httpCtxService);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<ChangePasswordCommand>(c => c.UserId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── LoginVisualStandard ──────────────────────────────────────────────

        [Fact]
        public async Task LoginVisualStandard_HandlerReturnsFailure_ReturnsUnauthorized()
        {
            // Arrange
            var sut     = new AuthController();
            var handler = Substitute.For<ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>>>();
            handler.HandleAsync(Arg.Any<VisualStandardLoginCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<VisualLoginResponse>.ErrorResult(ErrorCode.InvalidCredentials, "bad"));
            var request = new VisualStandardLoginRequest
            {
                UserId   = Guid.NewGuid(),
                Password = "wrong"
            };

            // Act
            var result = await sut.LoginVisualStandard(request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        // ── LoginWithPin ─────────────────────────────────────────────────────

        [Fact]
        public async Task LoginWithPin_HandlerReturnsFailure_ReturnsUnauthorized()
        {
            // Arrange
            var sut     = new AuthController();
            var handler = Substitute.For<ICommandHandler<PinLoginCommand, ApiResponse<VisualLoginResponse>>>();
            handler.HandleAsync(Arg.Any<PinLoginCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<VisualLoginResponse>.ErrorResult(ErrorCode.InvalidCredentials, "bad"));
            var request = new PinLoginRequest
            {
                UserId = Guid.NewGuid(),
                Pin    = "1234"
            };

            // Act
            var result = await sut.LoginWithPin(request, handler);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
