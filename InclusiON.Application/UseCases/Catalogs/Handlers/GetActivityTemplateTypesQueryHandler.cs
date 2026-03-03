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

            var response = items.Select(x => new ActivityTemplateTypeResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Code = x.Code,
                SkillAreaId = x.SkillAreaId,
                ContentSchema = x.ContentSchema,
                ComponentName = x.ComponentName,
                UsesPictograms = x.UsesPictograms,
                HasAudio = x.HasAudio,
                DisplayOrder = x.DisplayOrder
            }).ToList();

            return ApiResponse<List<ActivityTemplateTypeResponse>>.SuccessResult(response);
        }
    }
}
