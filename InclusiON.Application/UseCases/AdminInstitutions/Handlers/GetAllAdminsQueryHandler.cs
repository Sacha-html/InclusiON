using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class GetAllAdminsQueryHandler : IQueryHandler<GetAllAdminsQuery, ApiResponse<PagedResponse<AdminUserResponse>>>
    {
        private readonly IAdminInstitutionRepository _repository;

        public GetAllAdminsQueryHandler(IAdminInstitutionRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<AdminUserResponse>>> HandleAsync(
            GetAllAdminsQuery query, CancellationToken cancellationToken)
        {
            var paged = await _repository.GetAllAdminsPagedAsync(query.Page, query.PageSize, query.Search, cancellationToken);

            var response = new PagedResponse<AdminUserResponse>
            {
                Data            = paged.Data.Select(AdminInstitutionMapper.ToAdminUserResponse).ToList(),
                TotalRecords    = paged.TotalRecords,
                TotalPages      = paged.TotalPages,
                CurrentPage     = paged.CurrentPage,
                PageSize        = paged.PageSize,
                HasNextPage     = paged.HasNextPage,
                HasPreviousPage = paged.HasPreviousPage,
            };

            return ApiResponse<PagedResponse<AdminUserResponse>>.SuccessResult(response);
        }
    }
}
