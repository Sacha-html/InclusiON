using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonSkillProfileQueryHandler
        : IQueryHandler<GetPersonSkillProfileQuery, ApiResponse<List<PersonSkillProfileResponse>>>
    {
        private readonly IPersonsRepository _persons;

        public GetPersonSkillProfileQueryHandler(IPersonsRepository persons)
        {
            _persons = persons;
        }

        public async Task<ApiResponse<List<PersonSkillProfileResponse>>> HandleAsync(
            GetPersonSkillProfileQuery query, CancellationToken cancellationToken)
        {
            var profiles = await _persons.GetSkillProfileAsync(
                query.PersonId, activeOnly: !query.All, cancellationToken);

            var response = profiles.Select(PersonMapper.ToSkillProfileResponse).ToList();

            return ApiResponse<List<PersonSkillProfileResponse>>.SuccessResult(response);
        }
    }
}
