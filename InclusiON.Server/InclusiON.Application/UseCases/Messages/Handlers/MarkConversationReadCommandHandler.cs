using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class MarkConversationReadCommandHandler
        : ICommandHandler<MarkConversationReadCommand, ApiResponse<MarkReadResponse>>
    {
        private readonly IMessagesRepository _messages;
        private readonly IUnitOfWork         _uow;

        public MarkConversationReadCommandHandler(IMessagesRepository messages, IUnitOfWork uow)
        {
            _messages = messages;
            _uow      = uow;
        }

        public async Task<ApiResponse<MarkReadResponse>> HandleAsync(
            MarkConversationReadCommand command, CancellationToken cancellationToken)
        {
            var markedCount = await _messages.MarkConversationAsReadAsync(
                command.CurrentUserId, command.ContactUserId, cancellationToken);

            if (markedCount > 0)
            {
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var response = new MarkReadResponse { MarkedCount = markedCount };
            return ApiResponse<MarkReadResponse>.SuccessResult(response, $"{markedCount} mensaje(s) marcado(s) como leído(s).");
        }
    }
}
