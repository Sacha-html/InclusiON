using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class GetProfessionalsQueryHandler : IQueryHandler<GetProfessionalsQuery, ApiResponse<PagedResponse<ProfessionalListItemResponse>>>
    {
        private readonly IProfessionalsRepository _repository;

        public GetProfessionalsQueryHandler(IProfessionalsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<ProfessionalListItemResponse>>> HandleAsync(
            GetProfessionalsQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _repository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.Specialty,
                query.IsActive,
                query.SortBy,
                query.SortDirection,
                query.InstitutionId,
                cancellationToken);

            var response = new PagedResponse<ProfessionalListItemResponse>
            {
                Data = pagedResult.Data.Select(p => new ProfessionalListItemResponse
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DocumentNumber = p.DocumentNumber,
                    Phone = p.Phone,
                    Specialty = p.Specialty,
                    LicenseNumber = p.LicenseNumber,
                    IsActive = p.User?.IsActive ?? false,
                    Email = p.User?.Email
                }).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<ProfessionalListItemResponse>>.SuccessResult(response);
        }
    }
}
