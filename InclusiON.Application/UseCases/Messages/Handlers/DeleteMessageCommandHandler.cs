using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class DeleteMessageCommandHandler
        : ICommandHandler<DeleteMessageCommand, ApiResponse<object>>
    {
        private readonly IMessagesRepository _messages;
        private readonly IUnitOfWork         _uow;

        public DeleteMessageCommandHandler(IMessagesRepository messages, IUnitOfWork uow)
        {
            _messages = messages;
            _uow      = uow;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            DeleteMessageCommand command, CancellationToken cancellationToken)
        {
            var message = await _messages.GetByIdAsync(command.MessageId, cancellationToken);

            if (message is null || !message.IsActive)
                return ApiResponse<object>.NotFound("Mensaje");

            // Solo el remitente o el destinatario puede eliminar
            if (message.SenderId != command.UserId && message.ReceiverId != command.UserId)
                return ApiResponse<object>.Forbidden();

            // Soft delete
            message.IsActive = false;
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.SuccessResult("Mensaje eliminado exitosamente.");
        }
    }
}
