using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetActivityTemplateTypesQueryHandler
        : IQueryHandler<GetActivityTemplateTypesQuery, ApiResponse<List<ActivityTemplateTypeResponse>>>
    {
        private readonly IReadOnlyRepository<ActivityTemplateType> _repository;

        public GetActivityTemplateTypesQueryHandler(IReadOnlyRepository<ActivityTemplateType> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ActivityTemplateTypeResponse>>> HandleAsync(
            GetActivityTemplateTypesQuery query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetAllActiveAsync(cancellationToken);

            var response = items.Select(ActivityTemplateTypeResponse.MapToResponse).ToList();

            return ApiResponse<List<ActivityTemplateTypeResponse>>.SuccessResult(response);
        }
    }
}
