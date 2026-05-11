using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class GetAvailableFamiliesQueryHandler : IQueryHandler<GetAvailableFamiliesQuery, ApiResponse<PagedResponse<FamilyResponse>>>
    {
        private readonly IFamilyRepository _familyRepository;

        public GetAvailableFamiliesQueryHandler(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<PagedResponse<FamilyResponse>>> HandleAsync(GetAvailableFamiliesQuery query, CancellationToken cancellationToken)
        {
            var (families, totalRecords) = await _familyRepository.GetAvailableFamiliesAsync(
                query.Search, query.PersonId, query.Page, query.PageSize, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize);

            var response = new PagedResponse<FamilyResponse>
            {
                Data            = families.Select(f => FamilyResponse.MapToResponse(f.Family, f.WasPreviouslyLinked)).ToList(),
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<FamilyResponse>>.SuccessResult(response);
        }
    }
}
