using InclusiON.Application.Interfaces.Common;
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

        public GetActivitiesQueryHandler(IActivitiesRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<ActivityListItemResponse>>> HandleAsync(
            GetActivitiesQuery query, CancellationToken cancellationToken)
        {
            var skip = (query.Page - 1) * query.PageSize;

            var (items, total) = await _repository.GetPagedAsync(
                query.ProfessionalId,
                query.Search,
                query.CategoryId,
                query.SkillAreaId,
                query.TemplateTypeId,
                query.IsActive,
                query.IsStandard,
                skip,
                query.PageSize,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)total / query.PageSize);

            var paged = new PagedResponse<ActivityListItemResponse>
            {
                Data            = items.Select(ActivityListItemResponse.From).ToList(),
                TotalRecords    = total,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1,
            };

            return ApiResponse<PagedResponse<ActivityListItemResponse>>.SuccessResult(paged);
        }
    }
}
