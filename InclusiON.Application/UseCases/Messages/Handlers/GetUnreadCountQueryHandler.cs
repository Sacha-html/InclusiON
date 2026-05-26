using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class GetUnreadCountQueryHandler
        : IQueryHandler<GetUnreadCountQuery, ApiResponse<UnreadCountResponse>>
    {
        private readonly IMessagesRepository _messages;

        public GetUnreadCountQueryHandler(IMessagesRepository messages)
        {
            _messages = messages;
        }

        public async Task<ApiResponse<UnreadCountResponse>> HandleAsync(
            GetUnreadCountQuery query, CancellationToken cancellationToken)
        {
            var count = await _messages.GetUnreadCountAsync(query.UserId, cancellationToken);
            return ApiResponse<UnreadCountResponse>.SuccessResult(new UnreadCountResponse { Count = count });
        }
    }
}
