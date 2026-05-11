using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Messages;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Tests.Unit.Controllers
{
    /// <summary>
    /// Verifica que <see cref="MessagesController"/> requiere un userId valido en todos
    /// sus endpoints y que lo propaga correctamente a los handlers.
    /// </summary>
    public class MessagesControllerTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static MessagesController BuildSut(Guid? userId)
        {
            var httpCtx = Substitute.For<IHttpContextService>();
            httpCtx.GetCurrentUserId().Returns(userId);
            return new MessagesController(httpCtx);
        }

        // ── Handler factories ────────────────────────────────────────────────

        private static IQueryHandler<GetInboxQuery, ApiResponse<PagedResponse<MessageListItemResponse>>> OkInboxHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetInboxQuery, ApiResponse<PagedResponse<MessageListItemResponse>>>>();
            handler.HandleAsync(Arg.Any<GetInboxQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<PagedResponse<MessageListItemResponse>>.SuccessResult(new PagedResponse<MessageListItemResponse>()));
            return handler;
        }

        private static IQueryHandler<GetSentQuery, ApiResponse<PagedResponse<MessageListItemResponse>>> OkSentHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetSentQuery, ApiResponse<PagedResponse<MessageListItemResponse>>>>();
            handler.HandleAsync(Arg.Any<GetSentQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<PagedResponse<MessageListItemResponse>>.SuccessResult(new PagedResponse<MessageListItemResponse>()));
            return handler;
        }

        private static IQueryHandler<GetMessageByIdQuery, ApiResponse<MessageResponse>> OkGetMessageHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetMessageByIdQuery, ApiResponse<MessageResponse>>>();
            handler.HandleAsync(Arg.Any<GetMessageByIdQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<MessageResponse>.SuccessResult(new MessageResponse()));
            return handler;
        }

        private static IQueryHandler<GetMessageContactsQuery, ApiResponse<PagedResponse<MessageContactResponse>>> OkContactsHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetMessageContactsQuery, ApiResponse<PagedResponse<MessageContactResponse>>>>();
            handler.HandleAsync(Arg.Any<GetMessageContactsQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<PagedResponse<MessageContactResponse>>.SuccessResult(new PagedResponse<MessageContactResponse>()));
            return handler;
        }

        private static IQueryHandler<GetUnreadCountQuery, ApiResponse<UnreadCountResponse>> OkUnreadCountHandler()
        {
            var handler = Substitute.For<IQueryHandler<GetUnreadCountQuery, ApiResponse<UnreadCountResponse>>>();
            handler.HandleAsync(Arg.Any<GetUnreadCountQuery>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<UnreadCountResponse>.SuccessResult(new UnreadCountResponse()));
            return handler;
        }

        private static ICommandHandler<SendMessageCommand, ApiResponse<MessageResponse>> OkSendMessageHandler()
        {
            var handler = Substitute.For<ICommandHandler<SendMessageCommand, ApiResponse<MessageResponse>>>();
            handler.HandleAsync(Arg.Any<SendMessageCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<MessageResponse>.SuccessResult(new MessageResponse()));
            return handler;
        }

        private static ICommandHandler<ReplyToMessageCommand, ApiResponse<MessageResponse>> OkReplyHandler()
        {
            var handler = Substitute.For<ICommandHandler<ReplyToMessageCommand, ApiResponse<MessageResponse>>>();
            handler.HandleAsync(Arg.Any<ReplyToMessageCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<MessageResponse>.SuccessResult(new MessageResponse()));
            return handler;
        }

        private static ICommandHandler<MarkMessageReadCommand, ApiResponse<MessageResponse>> OkMarkReadHandler()
        {
            var handler = Substitute.For<ICommandHandler<MarkMessageReadCommand, ApiResponse<MessageResponse>>>();
            handler.HandleAsync(Arg.Any<MarkMessageReadCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<MessageResponse>.SuccessResult(new MessageResponse()));
            return handler;
        }

        private static ICommandHandler<DeleteMessageCommand, ApiResponse<object>> OkDeleteHandler()
        {
            var handler = Substitute.For<ICommandHandler<DeleteMessageCommand, ApiResponse<object>>>();
            handler.HandleAsync(Arg.Any<DeleteMessageCommand>(), Arg.Any<CancellationToken>())
                   .Returns(ApiResponse<object>.SuccessResult(new object()));
            return handler;
        }

        // ── GetInbox ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetInbox_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.GetInbox(OkInboxHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetInbox_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId  = Guid.NewGuid();
            var handler = OkInboxHandler();
            var sut     = BuildSut(userId: userId);

            // Act
            await sut.GetInbox(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetInboxQuery>(q => q.UserId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── GetSent ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetSent_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.GetSent(OkSentHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetSent_ValidUserId_PassesUserIdToHandler()
        {
            // Arrange
            var userId  = Guid.NewGuid();
            var handler = OkSentHandler();
            var sut     = BuildSut(userId: userId);

            // Act
            await sut.GetSent(handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetSentQuery>(q => q.UserId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── GetMessage ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMessage_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.GetMessage(42, OkGetMessageHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetMessage_ValidUserId_PassesMessageIdAndUserIdToHandler()
        {
            // Arrange
            var userId    = Guid.NewGuid();
            var messageId = 42;
            var handler   = OkGetMessageHandler();
            var sut       = BuildSut(userId: userId);

            // Act
            await sut.GetMessage(messageId, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<GetMessageByIdQuery>(q => q.MessageId == messageId && q.RequestingUserId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── GetContacts ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetContacts_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.GetContacts(OkContactsHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── GetUnreadCount ───────────────────────────────────────────────────

        [Fact]
        public async Task GetUnreadCount_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.GetUnreadCount(OkUnreadCountHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── SendMessage ──────────────────────────────────────────────────────

        [Fact]
        public async Task SendMessage_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut     = BuildSut(userId: null);
            var request = new SendMessageRequest { ReceiverId = Guid.NewGuid(), Subject = "Test", Content = "Hello" };

            // Act
            var result = await sut.SendMessage(request, OkSendMessageHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task SendMessage_ValidUserId_PassesSenderIdToHandler()
        {
            // Arrange
            var userId  = Guid.NewGuid();
            var handler = OkSendMessageHandler();
            var sut     = BuildSut(userId: userId);
            var request = new SendMessageRequest { ReceiverId = Guid.NewGuid(), Subject = "Test", Content = "Hello" };

            // Act
            await sut.SendMessage(request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<SendMessageCommand>(c => c.SenderId == userId),
                Arg.Any<CancellationToken>());
        }

        // ── ReplyToMessage ───────────────────────────────────────────────────

        [Fact]
        public async Task ReplyToMessage_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut     = BuildSut(userId: null);
            var request = new ReplyToMessageRequest { Content = "Reply" };

            // Act
            var result = await sut.ReplyToMessage(1, request, OkReplyHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ReplyToMessage_ValidUserId_PassesUserIdAndMessageIdToHandler()
        {
            // Arrange
            var userId    = Guid.NewGuid();
            var messageId = 99;
            var handler   = OkReplyHandler();
            var sut       = BuildSut(userId: userId);
            var request   = new ReplyToMessageRequest { Content = "Reply" };

            // Act
            await sut.ReplyToMessage(messageId, request, handler);

            // Assert
            await handler.Received(1).HandleAsync(
                Arg.Is<ReplyToMessageCommand>(c => c.SenderId == userId && c.ParentMessageId == messageId),
                Arg.Any<CancellationToken>());
        }

        // ── MarkAsRead ───────────────────────────────────────────────────────

        [Fact]
        public async Task MarkAsRead_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.MarkAsRead(1, OkMarkReadHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // ── DeleteMessage ────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteMessage_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = BuildSut(userId: null);

            // Act
            var result = await sut.DeleteMessage(1, OkDeleteHandler());

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
