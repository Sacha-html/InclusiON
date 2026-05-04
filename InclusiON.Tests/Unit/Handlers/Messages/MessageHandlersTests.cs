using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Application.UseCases.Messages.Handlers;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Messages
{
    // ════════════════════════════════════════════════════════════════════════════
    // Helpers compartidos
    // ════════════════════════════════════════════════════════════════════════════

    file static class Helpers
    {
        public static User AUser(Guid id, string name = "Juan", string surname = "Pérez") =>
            new() { Id = id, Name = name, Surname = surname, IsActive = true };

        public static Message AMessage(
            int id,
            Guid senderId, Guid receiverId,
            bool isRead = false, bool isActive = true,
            int? parentId = null) =>
            new()
            {
                Id              = id,
                SenderId        = senderId,
                ReceiverId      = receiverId,
                Subject         = "Asunto",
                Content         = "Contenido del mensaje",
                SentAt          = DateTime.UtcNow,
                IsRead          = isRead,
                IsActive        = isActive,
                ParentMessageId = parentId,
                Sender          = AUser(senderId, "Ana", "García"),
                Receiver        = AUser(receiverId, "Luis", "López"),
                Replies         = new List<Message>()
            };
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetInboxQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetInboxQueryHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private GetInboxQueryHandler BuildSut() => new(_messages);

        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_EmptyInbox_ReturnsEmptyPaged()
        {
            _messages.GetInboxAsync(UserId, 0, 20, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                     .Returns((new List<Message>(), 0));

            var result = await BuildSut().HandleAsync(new GetInboxQuery(UserId, 1, 20), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
            result.Data.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task HandleAsync_WithMessages_MapsFields()
        {
            var senderId   = Guid.NewGuid();
            var msg        = Helpers.AMessage(1, senderId, UserId);

            _messages.GetInboxAsync(UserId, 0, 20, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                     .Returns((new List<Message> { msg }, 1));

            var result = await BuildSut().HandleAsync(new GetInboxQuery(UserId, 1, 20), default);

            result.Data!.Data.Should().HaveCount(1);
            result.Data.Data[0].Id.Should().Be(1);
            result.Data.Data[0].SenderFullName.Should().Be("Ana García");
            result.Data.Data[0].ReceiverFullName.Should().Be("Luis López");
        }

        [Fact]
        public async Task HandleAsync_CalculatesPagination()
        {
            var items = Enumerable.Range(1, 5)
                .Select(i => Helpers.AMessage(i, Guid.NewGuid(), UserId))
                .ToList();

            _messages.GetInboxAsync(UserId, 0, 5, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                     .Returns((items, 12));

            var result = await BuildSut().HandleAsync(new GetInboxQuery(UserId, 1, 5), default);

            result.Data!.TotalRecords.Should().Be(12);
            result.Data.TotalPages.Should().Be(3);
            result.Data.HasNextPage.Should().BeTrue();
            result.Data.HasPreviousPage.Should().BeFalse();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetSentQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetSentQueryHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private GetSentQueryHandler BuildSut() => new(_messages);

        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_EmptySent_ReturnsEmptyPaged()
        {
            _messages.GetSentAsync(UserId, 0, 20, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                     .Returns((new List<Message>(), 0));

            var result = await BuildSut().HandleAsync(new GetSentQuery(UserId, 1, 20), default);

            result.Success.Should().BeTrue();
            result.Data!.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_SkipCalculatedCorrectly()
        {
            _messages.GetSentAsync(UserId, 40, 20, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                     .Returns((new List<Message>(), 0));

            await BuildSut().HandleAsync(new GetSentQuery(UserId, Page: 3, PageSize: 20), default);

            await _messages.Received(1).GetSentAsync(UserId, 40, 20, Arg.Any<bool?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetMessageByIdQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetMessageByIdQueryHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private readonly IUnitOfWork         _uow      = Substitute.For<IUnitOfWork>();
        private GetMessageByIdQueryHandler BuildSut() => new(_messages, _uow);

        private static readonly Guid SenderId   = Guid.NewGuid();
        private static readonly Guid ReceiverId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_MessageNotFound_ReturnsNotFound()
        {
            _messages.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Message?)null);

            var result = await BuildSut().HandleAsync(
                new GetMessageByIdQuery(99, SenderId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_InactiveMessage_ReturnsNotFound()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId, isActive: false));

            var result = await BuildSut().HandleAsync(
                new GetMessageByIdQuery(1, SenderId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_UserNotParticipant_ReturnsForbidden()
        {
            var outsider = Guid.NewGuid();
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId));

            var result = await BuildSut().HandleAsync(
                new GetMessageByIdQuery(1, outsider), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_ReceiverReads_MarksAsRead()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId, isRead: false);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            var result = await BuildSut().HandleAsync(
                new GetMessageByIdQuery(1, ReceiverId), default);

            result.Success.Should().BeTrue();
            msg.IsRead.Should().BeTrue();
            msg.ReadAt.Should().NotBeNull();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AlreadyRead_DoesNotSaveAgain()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId, isRead: true);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            await BuildSut().HandleAsync(new GetMessageByIdQuery(1, ReceiverId), default);

            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_SenderReads_DoesNotMarkAsRead()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId, isRead: false);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            await BuildSut().HandleAsync(new GetMessageByIdQuery(1, SenderId), default);

            msg.IsRead.Should().BeFalse();
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetUnreadCountQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetUnreadCountQueryHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private GetUnreadCountQueryHandler BuildSut() => new(_messages);

        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_ReturnsCorrectCount()
        {
            _messages.GetUnreadCountAsync(UserId, Arg.Any<CancellationToken>()).Returns(7);

            var result = await BuildSut().HandleAsync(new GetUnreadCountQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(7);
        }

        [Fact]
        public async Task HandleAsync_ZeroUnread_ReturnsZero()
        {
            _messages.GetUnreadCountAsync(UserId, Arg.Any<CancellationToken>()).Returns(0);

            var result = await BuildSut().HandleAsync(new GetUnreadCountQuery(UserId), default);

            result.Data!.Count.Should().Be(0);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // SendMessageCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class SendMessageCommandHandlerTests
    {
        private readonly IMessagesRepository    _messages    = Substitute.For<IMessagesRepository>();
        private readonly IUsersRepository       _users       = Substitute.For<IUsersRepository>();
        private readonly IAssignmentsRepository _assignments = Substitute.For<IAssignmentsRepository>();
        private readonly IUnitOfWork            _uow         = Substitute.For<IUnitOfWork>();

        private SendMessageCommandHandler BuildSut() => new(_messages, _users, _assignments, _uow);

        private static readonly Guid ProfUserId   = Guid.NewGuid();
        private static readonly Guid FamilyUserId = Guid.NewGuid();

        // Usuarios con perfil de profesional / familiar para tests de relación
        private static User AProfessionalUser(Guid id, string name = "Ana", string surname = "Gómez") =>
            new() { Id = id, Name = name, Surname = surname, IsActive = true,
                    Professional = new Professional { Id = Guid.NewGuid(), UserId = id } };

        private static User AFamilyUser(Guid id, string name = "Carlos", string surname = "López") =>
            new() { Id = id, Name = name, Surname = surname, IsActive = true,
                    FamilyRepresentative = new FamilyRepresentative { Id = Guid.NewGuid(), UserId = id } };

        private static User APersonUser(Guid id) =>
            new() { Id = id, IsActive = true,
                    PersonWithDisability = new PersonWithDisability { Id = Guid.NewGuid() } };

        private void SetupSharedPerson(bool share = true) =>
            _assignments.HaveSharedPersonAsync(ProfUserId, FamilyUserId, Arg.Any<CancellationToken>())
                        .Returns(share);

        // ── Validaciones de entrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmptyContent_ReturnsInvalidInput()
        {
            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "  ", null), default);

            result.ErrorCode.Should().Be(ErrorCode.InvalidInput);
        }

        [Fact]
        public async Task HandleAsync_SelfMessage_ReturnsInvalidInput()
        {
            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, ProfUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.InvalidInput);
        }

        // ── Validaciones de usuario ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_SenderNotFound_ReturnsNotFound()
        {
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>()).Returns((User?)null);

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ReceiverNotFound_ReturnsNotFound()
        {
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns((User?)null);

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ReceiverInactive_ReturnsNotFound()
        {
            var inactive = AFamilyUser(FamilyUserId);
            inactive.IsActive = false;
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(inactive);

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Validaciones de canal ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_SenderIsPersonWithDisability_ReturnsForbidden()
        {
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(APersonUser(ProfUserId));
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(FamilyUserId));

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_ReceiverIsPersonWithDisability_ReturnsForbidden()
        {
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(APersonUser(FamilyUserId));

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_BothProfessionals_ReturnsForbidden()
        {
            var anotherProfId = Guid.NewGuid();
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            _users.GetByIdWithProfileAsync(anotherProfId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(anotherProfId));

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, anotherProfId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_BothFamilyReps_ReturnsForbidden()
        {
            var anotherFamilyId = Guid.NewGuid();
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(FamilyUserId));
            _users.GetByIdWithProfileAsync(anotherFamilyId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(anotherFamilyId));

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(FamilyUserId, anotherFamilyId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        // ── Validación de relación compartida ─────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoSharedPerson_ReturnsForbidden()
        {
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(FamilyUserId));
            SetupSharedPerson(share: false);

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, null, "Hola", null), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        // ── Éxito ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfToFamily_SharedPerson_CreatesMessage()
        {
            var sender   = AProfessionalUser(ProfUserId, "Ana", "Gómez");
            var receiver = AFamilyUser(FamilyUserId, "Carlos", "López");
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>()).Returns(sender);
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>()).Returns(receiver);
            SetupSharedPerson(share: true);
            _messages.CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
                     .Returns(ci => ci.Arg<Message>());

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(ProfUserId, FamilyUserId, "Asunto", "Hola!", null), default);

            result.Success.Should().BeTrue();
            result.Data!.SenderId.Should().Be(ProfUserId);
            result.Data.ReceiverId.Should().Be(FamilyUserId);
            result.Data.SenderFullName.Should().Be("Ana Gómez");
            result.Data.ReceiverFullName.Should().Be("Carlos López");
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_FamilyToProf_ChecksSharedPersonCorrectly()
        {
            // Sender = family, Receiver = professional → HaveSharedPersonAsync(profUserId, familyUserId)
            _users.GetByIdWithProfileAsync(FamilyUserId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(FamilyUserId));
            _users.GetByIdWithProfileAsync(ProfUserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(ProfUserId));
            // Profesional es receiver, familiar es sender → args invertidos en el método
            _assignments.HaveSharedPersonAsync(ProfUserId, FamilyUserId, Arg.Any<CancellationToken>())
                        .Returns(true);
            _messages.CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
                     .Returns(ci => ci.Arg<Message>());

            var result = await BuildSut().HandleAsync(
                new SendMessageCommand(FamilyUserId, ProfUserId, null, "Consulta", null), default);

            result.Success.Should().BeTrue();
            await _assignments.Received(1)
                .HaveSharedPersonAsync(ProfUserId, FamilyUserId, Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // ReplyToMessageCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class ReplyToMessageCommandHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private readonly IUsersRepository    _users    = Substitute.For<IUsersRepository>();
        private readonly IUnitOfWork         _uow      = Substitute.For<IUnitOfWork>();
        private ReplyToMessageCommandHandler BuildSut() => new(_messages, _users, _uow);

        private static readonly Guid SenderId   = Guid.NewGuid();
        private static readonly Guid ReceiverId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_EmptyContent_ReturnsInvalidInput()
        {
            var result = await BuildSut().HandleAsync(
                new ReplyToMessageCommand(SenderId, 1, "   "), default);

            result.ErrorCode.Should().Be(ErrorCode.InvalidInput);
        }

        [Fact]
        public async Task HandleAsync_ParentNotFound_ReturnsNotFound()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Message?)null);

            var result = await BuildSut().HandleAsync(
                new ReplyToMessageCommand(SenderId, 1, "Respuesta"), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_UserNotParticipant_ReturnsForbidden()
        {
            var outsider = Guid.NewGuid();
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId));

            var result = await BuildSut().HandleAsync(
                new ReplyToMessageCommand(outsider, 1, "Respuesta"), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_ValidReply_SetsCorrectReceiver()
        {
            // El original fue: sender → receiver. Quien responde es receiver, entonces el nuevo receptor es sender.
            var parent   = Helpers.AMessage(1, SenderId, ReceiverId);
            var replier  = Helpers.AUser(ReceiverId, "Luis", "López");
            var original = Helpers.AUser(SenderId, "Ana", "García");

            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(parent);
            _users.GetByIdAsync(ReceiverId, Arg.Any<CancellationToken>()).Returns(replier);
            _users.GetByIdAsync(SenderId, Arg.Any<CancellationToken>()).Returns(original);
            _messages.CreateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
                     .Returns(ci => ci.Arg<Message>());

            var result = await BuildSut().HandleAsync(
                new ReplyToMessageCommand(ReceiverId, 1, "Entendido"), default);

            result.Success.Should().BeTrue();
            result.Data!.SenderId.Should().Be(ReceiverId);
            result.Data.ReceiverId.Should().Be(SenderId);   // receptor = remitente original
            result.Data.ParentMessageId.Should().Be(1);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // MarkMessageReadCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class MarkMessageReadCommandHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private readonly IUnitOfWork         _uow      = Substitute.For<IUnitOfWork>();
        private MarkMessageReadCommandHandler BuildSut() => new(_messages, _uow);

        private static readonly Guid SenderId   = Guid.NewGuid();
        private static readonly Guid ReceiverId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_MessageNotFound_ReturnsNotFound()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Message?)null);

            var result = await BuildSut().HandleAsync(new MarkMessageReadCommand(1, ReceiverId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_NotReceiver_ReturnsForbidden()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId));

            var result = await BuildSut().HandleAsync(new MarkMessageReadCommand(1, SenderId), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_AlreadyRead_ReturnsConflict()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId, isRead: true));

            var result = await BuildSut().HandleAsync(new MarkMessageReadCommand(1, ReceiverId), default);

            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_MarksReadAndSaves()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId, isRead: false);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            var result = await BuildSut().HandleAsync(new MarkMessageReadCommand(1, ReceiverId), default);

            result.Success.Should().BeTrue();
            msg.IsRead.Should().BeTrue();
            msg.ReadAt.Should().NotBeNull();
            result.Data!.IsRead.Should().BeTrue();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DeleteMessageCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class DeleteMessageCommandHandlerTests
    {
        private readonly IMessagesRepository _messages = Substitute.For<IMessagesRepository>();
        private readonly IUnitOfWork         _uow      = Substitute.For<IUnitOfWork>();
        private DeleteMessageCommandHandler BuildSut() => new(_messages, _uow);

        private static readonly Guid SenderId   = Guid.NewGuid();
        private static readonly Guid ReceiverId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_MessageNotFound_ReturnsNotFound()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Message?)null);

            var result = await BuildSut().HandleAsync(new DeleteMessageCommand(1, SenderId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_Outsider_ReturnsForbidden()
        {
            var outsider = Guid.NewGuid();
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId));

            var result = await BuildSut().HandleAsync(new DeleteMessageCommand(1, outsider), default);

            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task HandleAsync_SenderDeletes_SoftDeletes()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            var result = await BuildSut().HandleAsync(new DeleteMessageCommand(1, SenderId), default);

            result.Success.Should().BeTrue();
            msg.IsActive.Should().BeFalse();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_ReceiverDeletes_SoftDeletes()
        {
            var msg = Helpers.AMessage(1, SenderId, ReceiverId);
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(msg);

            var result = await BuildSut().HandleAsync(new DeleteMessageCommand(1, ReceiverId), default);

            result.Success.Should().BeTrue();
            msg.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_InactiveMessage_ReturnsNotFound()
        {
            _messages.GetByIdAsync(1, Arg.Any<CancellationToken>())
                     .Returns(Helpers.AMessage(1, SenderId, ReceiverId, isActive: false));

            var result = await BuildSut().HandleAsync(new DeleteMessageCommand(1, SenderId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetMessageContactsQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetMessageContactsQueryHandlerTests
    {
        private readonly IUsersRepository       _users       = Substitute.For<IUsersRepository>();
        private readonly IAssignmentsRepository _assignments = Substitute.For<IAssignmentsRepository>();

        private static readonly Guid UserId    = Guid.NewGuid();
        private static readonly Guid ContactId = Guid.NewGuid();

        private GetMessageContactsQueryHandler BuildSut() => new(_users, _assignments);

        private static User AProfessionalUser(Guid id) => new()
        {
            Id = id, Name = "Ana", Surname = "Gómez", Email = "ana@test.com",
            IsActive = true, Professional = new Professional { Id = Guid.NewGuid(), UserId = id }
        };

        private static User AFamilyUser(Guid id) => new()
        {
            Id = id, Name = "Luis", Surname = "López", Email = "luis@test.com",
            IsActive = true, FamilyRepresentative = new FamilyRepresentative { Id = Guid.NewGuid(), UserId = id }
        };

        private static User APersonUser(Guid id) => new()
        {
            Id = id, Name = "María", Surname = "Pérez", Email = "maria@test.com",
            IsActive = true, PersonWithDisability = new PersonWithDisability { Id = Guid.NewGuid() }
        };

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsNotFound()
        {
            _users.GetByIdWithProfileAsync(UserId, Arg.Any<CancellationToken>())
                  .Returns((User?)null);

            var result = await BuildSut().HandleAsync(new GetMessageContactsQuery(UserId), default);

            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_PersonWithDisability_ReturnsEmptyList()
        {
            _users.GetByIdWithProfileAsync(UserId, Arg.Any<CancellationToken>())
                  .Returns(APersonUser(UserId));

            var result = await BuildSut().HandleAsync(new GetMessageContactsQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_Professional_ReturnsFamilyContacts()
        {
            _users.GetByIdWithProfileAsync(UserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(UserId));

            var familyUser = AFamilyUser(ContactId);
            _assignments.GetContactsForProfessionalAsync(UserId, Arg.Any<CancellationToken>())
                        .Returns(new List<User> { familyUser });

            var result = await BuildSut().HandleAsync(new GetMessageContactsQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].UserId.Should().Be(ContactId);
            result.Data[0].UserType.Should().Be("FamilyRepresentative");
            result.Data[0].FullName.Should().Be("Luis López");
        }

        [Fact]
        public async Task HandleAsync_FamilyRepresentative_ReturnsProfessionalContacts()
        {
            _users.GetByIdWithProfileAsync(UserId, Arg.Any<CancellationToken>())
                  .Returns(AFamilyUser(UserId));

            var profUser = AProfessionalUser(ContactId);
            _assignments.GetContactsForFamilyAsync(UserId, Arg.Any<CancellationToken>())
                        .Returns(new List<User> { profUser });

            var result = await BuildSut().HandleAsync(new GetMessageContactsQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].UserId.Should().Be(ContactId);
            result.Data[0].UserType.Should().Be("Professional");
            result.Data[0].FullName.Should().Be("Ana Gómez");
        }

        [Fact]
        public async Task HandleAsync_Professional_NoContacts_ReturnsEmptyList()
        {
            _users.GetByIdWithProfileAsync(UserId, Arg.Any<CancellationToken>())
                  .Returns(AProfessionalUser(UserId));

            _assignments.GetContactsForProfessionalAsync(UserId, Arg.Any<CancellationToken>())
                        .Returns(new List<User>());

            var result = await BuildSut().HandleAsync(new GetMessageContactsQuery(UserId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
