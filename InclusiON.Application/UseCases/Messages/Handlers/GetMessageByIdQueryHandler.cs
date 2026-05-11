using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.Application.Mappers;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class GetMessageByIdQueryHandler
        : IQueryHandler<GetMessageByIdQuery, ApiResponse<MessageResponse>>
    {
        private readonly IMessagesRepository _messages;
        private readonly IUnitOfWork         _uow;
        private readonly IEncryptionService  _encryption;

        public GetMessageByIdQueryHandler(IMessagesRepository messages, IUnitOfWork uow, IEncryptionService encryption)
        {
            _messages   = messages;
            _uow        = uow;
            _encryption = encryption;
        }

        public async Task<ApiResponse<MessageResponse>> HandleAsync(
            GetMessageByIdQuery query, CancellationToken cancellationToken)
        {
            var message = await _messages.GetByIdAsync(query.MessageId, cancellationToken);

            if (message is null || !message.IsActive)
                return ApiResponse<MessageResponse>.NotFound("Mensaje");

            // Solo el remitente o el destinatario puede ver el mensaje
            if (message.SenderId != query.RequestingUserId &&
                message.ReceiverId != query.RequestingUserId)
                return ApiResponse<MessageResponse>.Forbidden();

            // Marcar como leído si el destinatario lo abre por primera vez
            if (!message.IsRead && message.ReceiverId == query.RequestingUserId)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var dto = MessageMapper.ToDetail(message);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(message.Id.ToString()));
            foreach (var reply in dto.Replies)
            {
                var domainReply = message.Replies.First(r => r.Id == reply.Id);
                reply.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(domainReply.Id.ToString()));
            }
            return ApiResponse<MessageResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
