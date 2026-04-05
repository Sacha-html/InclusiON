using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetSkillAreasQueryHandler
        : IQueryHandler<GetSkillAreasQuery, ApiResponse<List<SkillAreaResponse>>>
    {
        private readonly IReadOnlyRepository<SkillArea> _repository;

        public GetSkillAreasQueryHandler(IReadOnlyRepository<SkillArea> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<SkillAreaResponse>>> HandleAsync(
            GetSkillAreasQuery query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetAllActiveAsync(cancellationToken);

            var response = items.Select(SkillAreaResponse.MapToResponse).ToList();

            return ApiResponse<List<SkillAreaResponse>>.SuccessResult(response);
        }
    }
}
