using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonProfessionalsQueryHandler
        : IQueryHandler<GetPersonProfessionalsQuery, ApiResponse<List<PersonProfessionalResponse>>>
    {
        private readonly IAssignmentsRepository _assignments;

        public GetPersonProfessionalsQueryHandler(IAssignmentsRepository assignments)
        {
            _assignments = assignments;
        }

        public async Task<ApiResponse<List<PersonProfessionalResponse>>> HandleAsync(
            GetPersonProfessionalsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _assignments.GetProfessionalsByPersonIdAsync(
                query.PersonId, cancellationToken);

            var response = assignments.Select(PersonMapper.ToProfessionalResponse).ToList();

            return ApiResponse<List<PersonProfessionalResponse>>.SuccessResult(response);
        }
    }
}
