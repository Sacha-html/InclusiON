using System.Text.Json;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Application.Mappers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class ReplyToMessageCommandHandler
        : ICommandHandler<ReplyToMessageCommand, ApiResponse<MessageResponse>>
    {
        private readonly IMessagesRepository     _messages;
        private readonly IUsersRepository        _users;
        private readonly IUnitOfWork             _uow;
        private readonly IEncryptionService      _encryption;
        private readonly IBackgroundJobRepository _bgJobs;

        public ReplyToMessageCommandHandler(
            IMessagesRepository messages,
            IUsersRepository users,
            IUnitOfWork uow,
            IEncryptionService encryption,
            IBackgroundJobRepository bgJobs)
        {
            _messages   = messages;
            _users      = users;
            _uow        = uow;
            _encryption = encryption;
            _bgJobs     = bgJobs;
        }

        public async Task<ApiResponse<MessageResponse>> HandleAsync(
            ReplyToMessageCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Content))
                return ApiResponse<MessageResponse>.ErrorResult(
                    ErrorCode.InvalidInput, "El contenido de la respuesta no puede estar vacío.");

            // 1. Obtener el mensaje padre
            var parent = await _messages.GetByIdAsync(command.ParentMessageId, cancellationToken);
            if (parent is null || !parent.IsActive)
                return ApiResponse<MessageResponse>.NotFound("Mensaje");

            // 2. Solo participantes del hilo pueden responder
            if (parent.SenderId != command.SenderId && parent.ReceiverId != command.SenderId)
                return ApiResponse<MessageResponse>.Forbidden();

            // 3. El receptor de la respuesta es el otro participante del mensaje padre
            var receiverId = parent.SenderId == command.SenderId
                ? parent.ReceiverId
                : parent.SenderId;

            var sender   = await _users.GetByIdAsync(command.SenderId, cancellationToken);
            var receiver = await _users.GetByIdAsync(receiverId, cancellationToken);

            if (sender is null || receiver is null)
                return ApiResponse<MessageResponse>.NotFound("Usuario");

            // 4. Crear la respuesta (hereda subject y relatedPersonId del padre)
            // Note: do NOT assign Sender/Receiver navigation properties here.
            // Both were loaded via AsNoTracking; assigning them would make EF try to INSERT
            // existing User rows, causing a PK duplicate-key violation on SaveChangesAsync.
            // Setting only the FK fields (SenderId/ReceiverId) is sufficient.
            var reply = new Message
            {
                SenderId        = command.SenderId,
                ReceiverId      = receiverId,
                Subject         = parent.Subject,
                Content         = command.Content,
                RelatedPersonId = parent.RelatedPersonId,
                ParentMessageId = command.ParentMessageId,
                SentAt          = DateTime.UtcNow,
                IsRead          = false,
                IsActive        = true,
            };

            await _messages.CreateAsync(reply, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Assign navigation properties AFTER save — in-memory only, safe for DTO mapping.
            reply.Sender   = sender;
            reply.Receiver = receiver;

            // Push SignalR al destinatario — fire and forget
            var senderName    = $"{sender!.Name} {sender.Surname}".Trim();
            var receiverIdStr = receiver!.Id.ToString();
            _ = Task.Run(async () =>
            {
                await _bgJobs.CreateAsync(
                    JobTypes.Push,
                    JsonSerializer.Serialize(new NotificationPayload
                    {
                        UserId           = receiverIdStr,
                        Title            = "Nueva respuesta",
                        Message          = $"{senderName} respondió un mensaje.",
                        ActionUrl        = "/#/pro/messages",
                        SendEmailFallback = false
                    }),
                    maxRetries: 3);
            });

            var dto = MessageMapper.ToDetail(reply);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(reply.Id.ToString()));
            return ApiResponse<MessageResponse>.SuccessResult(dto, "Respuesta enviada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
