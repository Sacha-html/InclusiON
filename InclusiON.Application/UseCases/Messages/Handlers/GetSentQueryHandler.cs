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
    public class GetSentQueryHandler
        : IQueryHandler<GetSentQuery, ApiResponse<PagedResponse<MessageListItemResponse>>>
    {
        private readonly IMessagesRepository _messages;
        private readonly IEncryptionService  _encryption;

        public GetSentQueryHandler(IMessagesRepository messages, IEncryptionService encryption)
        {
            _messages   = messages;
            _encryption = encryption;
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
                Data = items.Select(m =>
                {
                    var item = MessageMapper.ToListItem(m);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(m.Id.ToString()));
                    return item;
                }).ToList(),
                TotalRecords    = total,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<MessageListItemResponse>>.SuccessResult(data);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
