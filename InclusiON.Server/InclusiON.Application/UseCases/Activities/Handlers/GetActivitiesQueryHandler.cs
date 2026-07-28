using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetActivitiesQueryHandler
        : IQueryHandler<GetActivitiesQuery, ApiResponse<PagedResponse<ActivityListItemResponse>>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetActivitiesQueryHandler(IActivitiesRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<PagedResponse<ActivityListItemResponse>>> HandleAsync(
            GetActivitiesQuery query, CancellationToken cancellationToken)
        {
            var (items, total) = await _repository.GetPagedAsync(
                query.ProfessionalId,
                query.Search,
                query.CategoryId,
                query.SkillAreaId,
                query.TemplateTypeId,
                query.IsActive,
                query.IsStandard,
                query.Page,
                query.PageSize,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)total / query.PageSize);

            var paged = new PagedResponse<ActivityListItemResponse>
            {
                Data = items.Select(a =>
                {
                    var item = ActivityListItemResponse.From(a);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(a.Id.ToString()));
                    return item;
                }).ToList(),
                TotalRecords    = total,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1,
            };

            return ApiResponse<PagedResponse<ActivityListItemResponse>>.SuccessResult(paged);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
