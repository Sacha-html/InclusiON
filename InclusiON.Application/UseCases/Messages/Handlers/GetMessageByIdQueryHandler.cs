using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Queries;
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

        public GetMessageByIdQueryHandler(IMessagesRepository messages, IUnitOfWork uow)
        {
            _messages = messages;
            _uow      = uow;
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

            return ApiResponse<MessageResponse>.SuccessResult(MessageMapper.ToDetail(message));
        }
    }
}
