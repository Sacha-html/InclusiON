using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class GetSentQueryHandler
        : IQueryHandler<GetSentQuery, ApiResponse<PagedResponse<MessageListItemResponse>>>
    {
        private readonly IMessagesRepository _messages;

        public GetSentQueryHandler(IMessagesRepository messages)
        {
            _messages = messages;
        }

        public async Task<ApiResponse<PagedResponse<MessageListItemResponse>>> HandleAsync(
            GetSentQuery query, CancellationToken cancellationToken)
        {
            var skip = (query.Page - 1) * query.PageSize;

            var (items, total) = await _messages.GetSentAsync(
                query.UserId, skip, query.PageSize,
                query.IsRead, query.RelatedPersonId, query.ReceiverId,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)total / query.PageSize);

            var data = new PagedResponse<MessageListItemResponse>
            {
                Data            = items.Select(MessageMapper.ToListItem).ToList(),
                TotalRecords    = total,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<MessageListItemResponse>>.SuccessResult(data);
        }
    }
}
