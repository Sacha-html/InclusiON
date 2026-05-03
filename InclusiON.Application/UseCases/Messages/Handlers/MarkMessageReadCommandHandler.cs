using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
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

        public MarkMessageReadCommandHandler(IMessagesRepository messages, IUnitOfWork uow)
        {
            _messages = messages;
            _uow      = uow;
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

            return ApiResponse<MessageResponse>.SuccessResult(
                MessageMapper.ToDetail(message),
                "Mensaje marcado como leído.");
        }
    }
}
