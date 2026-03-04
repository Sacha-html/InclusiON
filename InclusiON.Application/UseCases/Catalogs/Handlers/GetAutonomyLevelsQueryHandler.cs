using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetAutonomyLevelsQueryHandler
        : IQueryHandler<GetAutonomyLevelsQuery, ApiResponse<List<AutonomyLevelResponse>>>
    {
        private readonly IReadOnlyRepository<AutonomyLevel> _repository;

        public GetAutonomyLevelsQueryHandler(IReadOnlyRepository<AutonomyLevel> repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<AutonomyLevelResponse>>> HandleAsync(
            GetAutonomyLevelsQuery query, CancellationToken cancellationToken)
        {
            var items = await _repository.GetAllActiveAsync(cancellationToken);

            var response = items.Select(x => new AutonomyLevelResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                RequiresSupervision = x.RequiresSupervision,
                DisplayOrder = x.DisplayOrder
            }).ToList();

            return ApiResponse<List<AutonomyLevelResponse>>.SuccessResult(response);
        }
    }
}
