using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Tests.Controllers
{
    /// <summary>
    /// Verifica que AdminUsersController devuelve 401 cuando GetCurrentUserId() es null
    /// en lugar de lanzar NullReferenceException (bug crítico corregido: uso de !.Value sin guard).
    /// </summary>
    public class AdminUsersControllerTests
    {
        private static AdminUsersController BuildSut(Guid? userId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns(userId);
            return new AdminUsersController(httpCtx);
        }

        private static ICommandHandler<AdminResetPasswordCommand, ApiResponse<ResetPasswordResultResponse>> OkResetHandler()
        {
            var h = Substitute.For<ICommandHandler<AdminResetPasswordCommand, ApiResponse<ResetPasswordResultResponse>>>();
            h.HandleAsync(Arg.Any<AdminResetPasswordCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ResetPasswordResultResponse>.SuccessResult(new ResetPasswordResultResponse()));
            return h;
        }

        // ── ResetPassword ────────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_NullCurrentUser_Returns401()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.ResetPassword(Guid.NewGuid(), OkResetHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ResetPassword_ValidCurrentUser_PassesUserIdsToHandler()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var targetUserId  = Guid.NewGuid();
            var handler       = OkResetHandler();
            var sut           = BuildSut(userId: currentUserId);

            // Act
            await sut.ResetPassword(targetUserId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<AdminResetPasswordCommand>(c =>
                    c.UserId == targetUserId && c.RequestedByUserId == currentUserId),
                Arg.Any<CancellationToken>());
        }

    }
}
