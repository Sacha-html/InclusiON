using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Application.Mappers;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class MarkMessageReadCommandHandler
        : ICommandHandler<MarkMessageReadCommand, ApiResponse<MessageResponse>>
    {
        private readonly IMessagesRepository _messages;
        private readonly IUnitOfWork         _uow;
        private readonly IEncryptionService  _encryption;

        public MarkMessageReadCommandHandler(IMessagesRepository messages, IUnitOfWork uow, IEncryptionService encryption)
        {
            _messages   = messages;
            _uow        = uow;
            _encryption = encryption;
        }

        public async Task<ApiResponse<MessageResponse>> HandleAsync(
            MarkMessageReadCommand command, CancellationToken cancellationToken)
        {
            var message = await _messages.GetByIdAsync(command.MessageId, cancellationToken);

            if (message is null || !message.IsActive)
                return ApiResponse<MessageResponse>.NotFound("Mensaje");

            // Solo el destinatario puede marcar como leído
            if (message.ReceiverId != command.UserId)
                return ApiResponse<MessageResponse>.Forbidden();

            if (message.IsRead)
                return ApiResponse<MessageResponse>.Conflict(
                    ErrorCode.Conflict, "El mensaje ya estaba marcado como leído.");

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = MessageMapper.ToDetail(message);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(message.Id.ToString()));
            foreach (var reply in dto.Replies)
            {
                var domainReply = message.Replies.First(r => r.Id == reply.Id);
                reply.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(domainReply.Id.ToString()));
            }
            return ApiResponse<MessageResponse>.SuccessResult(dto, "Mensaje marcado como leído.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
