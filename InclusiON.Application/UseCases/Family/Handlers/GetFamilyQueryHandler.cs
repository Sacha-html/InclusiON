using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetFamilyQueryHandler : IQueryHandler<GetFamilyQuery, ApiResponse<PagedResponse<FamilyListItemResponse>>>
    {
        private readonly IFamilyRepository _repository;

        public GetFamilyQueryHandler(IFamilyRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<FamilyListItemResponse>>> HandleAsync(
            GetFamilyQuery query, CancellationToken cancellationToken)
        {
            var pagedResult = await _repository.GetPagedAsync(
                query.Page, query.PageSize, query.Search, query.IsActive,
                query.SortBy, query.SortDirection, query.InstitutionIds,
                cancellationToken);

            var response = new PagedResponse<FamilyListItemResponse>
            {
                Data = pagedResult.Data.Select(f => new FamilyListItemResponse
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    FirstName = f.FirstName,
                    LastName = f.LastName,
                    DocumentNumber = f.DocumentNumber,
                    Phone = f.Phone,
                    Relationship = f.Relationship,
                    IsActive = f.User?.IsActive ?? false,
                    Email = f.User?.Email
                }).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<FamilyListItemResponse>>.SuccessResult(response);
        }
    }
}
