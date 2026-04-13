using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonsQueryHandler : IQueryHandler<GetPersonsQuery, ApiResponse<PagedResponse<PersonListItemResponse>>>
    {
        private readonly IPersonsRepository _repository;

        public GetPersonsQueryHandler(IPersonsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<PersonListItemResponse>>> HandleAsync(
            GetPersonsQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _repository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.DisabilityTypeId,
                query.AutonomyLevelId,
                query.IsActive,
                query.SortBy,
                query.SortDirection,
                query.InstitutionIds,
                query.RepresentativeSearch,
                cancellationToken);

            var response = new PagedResponse<PersonListItemResponse>
            {
                Data = pagedResult.Data.Select(PersonListItemResponse.MapToResponse).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<PersonListItemResponse>>.SuccessResult(response);
        }
    }
}
