using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.AdminInstitutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class GetAdminInstitutionsQueryHandler : IQueryHandler<GetAdminInstitutionsQuery, ApiResponse<PagedResponse<AdminInstitutionResponse>>>
    {
        private readonly IAdminInstitutionRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetAdminInstitutionsQueryHandler(IAdminInstitutionRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<PagedResponse<AdminInstitutionResponse>>> HandleAsync(
            GetAdminInstitutionsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetInstitutionsByAdminAsync(query.AdminUserId, cancellationToken);

            var all = assignments.Select(a =>
            {
                var r = AdminInstitutionMapper.ToResponse(a);
                r.EncryptedInstitutionId = _encryption.EncryptId(a.InstitutionId);
                return r;
            }).ToList();

            var totalRecords = all.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)query.PageSize);
            var data         = all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

            var response = new PagedResponse<AdminInstitutionResponse>
            {
                Data            = data,
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<AdminInstitutionResponse>>.SuccessResult(response);
        }

    }
}
