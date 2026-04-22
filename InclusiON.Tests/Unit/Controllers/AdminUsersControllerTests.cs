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

namespace InclusiON.Tests.Unit.Controllers
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

        private static ICommandHandler<AdminDeactivateUserCommand, ApiResponse<object>> OkDeactivateHandler()
        {
            var h = Substitute.For<ICommandHandler<AdminDeactivateUserCommand, ApiResponse<object>>>();
            h.HandleAsync(Arg.Any<AdminDeactivateUserCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<object>.SuccessResult(new object()));
            return h;
        }

        private static ICommandHandler<AdminReactivateUserCommand, ApiResponse<ResetPasswordResultResponse>> OkReactivateHandler()
        {
            var h = Substitute.For<ICommandHandler<AdminReactivateUserCommand, ApiResponse<ResetPasswordResultResponse>>>();
            h.HandleAsync(Arg.Any<AdminReactivateUserCommand>(), Arg.Any<CancellationToken>())
             .Returns(ApiResponse<ResetPasswordResultResponse>.SuccessResult(new ResetPasswordResultResponse()));
            return h;
        }

        // ── ResetPassword ────────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_NullCurrentUser_Returns401()
        {
            var sut    = BuildSut(userId: null);
            var result = await sut.ResetPassword(Guid.NewGuid(), OkResetHandler());

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ResetPassword_ValidCurrentUser_PassesUserIdsToHandler()
        {
            var currentUserId = Guid.NewGuid();
            var targetUserId  = Guid.NewGuid();
            var handler       = OkResetHandler();
            var sut           = BuildSut(userId: currentUserId);

            await sut.ResetPassword(targetUserId, handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<AdminResetPasswordCommand>(c =>
                    c.UserId == targetUserId && c.RequestedByUserId == currentUserId),
                Arg.Any<CancellationToken>());
        }

        // ── DeactivateUser ───────────────────────────────────────────────────

        [Fact]
        public async Task DeactivateUser_NullCurrentUser_Returns401()
        {
            var sut    = BuildSut(userId: null);
            var result = await sut.DeactivateUser(Guid.NewGuid(), OkDeactivateHandler());

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task DeactivateUser_ValidCurrentUser_PassesUserIdsToHandler()
        {
            var currentUserId = Guid.NewGuid();
            var targetUserId  = Guid.NewGuid();
            var handler       = OkDeactivateHandler();
            var sut           = BuildSut(userId: currentUserId);

            await sut.DeactivateUser(targetUserId, handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<AdminDeactivateUserCommand>(c =>
                    c.UserId == targetUserId && c.RequestedByUserId == currentUserId),
                Arg.Any<CancellationToken>());
        }

        // ── ReactivateUser ───────────────────────────────────────────────────

        [Fact]
        public async Task ReactivateUser_NullCurrentUser_Returns401()
        {
            var sut    = BuildSut(userId: null);
            var result = await sut.ReactivateUser(Guid.NewGuid(), OkReactivateHandler());

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ReactivateUser_ValidCurrentUser_PassesUserIdsToHandler()
        {
            var currentUserId = Guid.NewGuid();
            var targetUserId  = Guid.NewGuid();
            var handler       = OkReactivateHandler();
            var sut           = BuildSut(userId: currentUserId);

            await sut.ReactivateUser(targetUserId, handler);

            await handler.Received(1).HandleAsync(
                Arg.Is<AdminReactivateUserCommand>(c =>
                    c.UserId == targetUserId && c.RequestedByUserId == currentUserId),
                Arg.Any<CancellationToken>());
        }
    }
}
