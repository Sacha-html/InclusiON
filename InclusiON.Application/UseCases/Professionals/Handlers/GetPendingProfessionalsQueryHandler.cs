using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class GetPendingProfessionalsQueryHandler : IQueryHandler<GetPendingProfessionalsQuery, ApiResponse<PagedResponse<ProfessionalListItemResponse>>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;
        private readonly IHttpContextService _httpContextService;

        public GetPendingProfessionalsQueryHandler(
            IProfessionalsRepository repository,
            IAdminInstitutionRepository adminInstitutionRepository,
            IHttpContextService httpContextService)
        {
            _repository = repository;
            _adminInstitutionRepository = adminInstitutionRepository;
            _httpContextService = httpContextService;
        }

        public async Task<ApiResponse<PagedResponse<ProfessionalListItemResponse>>> HandleAsync(
            GetPendingProfessionalsQuery query,
            CancellationToken cancellationToken)
        {
            var adminUserId = _httpContextService.GetCurrentUserId();
            List<int>? institutionIds = null;

            if (adminUserId.HasValue)
            {
                institutionIds = await _adminInstitutionRepository.GetActiveInstitutionIdsByAdminAsync(adminUserId.Value, cancellationToken);
            }

            var pagedResult = await _repository.GetPendingPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.SortBy,
                query.SortDirection,
                institutionIds,
                cancellationToken);

            var items = pagedResult.Data.Select(p => new ProfessionalListItemResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DocumentNumber = p.DocumentNumber,
                Phone = p.Phone,
                Email = !string.IsNullOrEmpty(p.Email) ? p.Email : (p.User != null ? p.User.Email : null),
                Specialty = p.Specialty,
                LicenseNumber = p.LicenseNumber,
                IsActive = p.User?.IsActive ?? false,
                CreatedAt = p.CreatedAt
            }).ToList();

            var response = new PagedResponse<ProfessionalListItemResponse>
            {
                Data = items,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<ProfessionalListItemResponse>>.SuccessResult(response);
        }
    }
}