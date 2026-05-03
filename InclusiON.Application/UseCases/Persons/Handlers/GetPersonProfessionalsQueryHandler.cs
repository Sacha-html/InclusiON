using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
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

            var response = assignments.Select(pp => new PersonProfessionalResponse
            {
                ProfessionalId        = pp.ProfessionalId,
                PersonId              = pp.PersonId,
                PersonFirstName       = pp.Professional.FirstName,
                PersonLastName        = pp.Professional.LastName,
                PersonFullName        = $"{pp.Professional.FirstName} {pp.Professional.LastName}",
                IsPrimaryProfessional = pp.IsPrimaryProfessional,
                CanSuperviseLogin     = pp.CanSuperviseLogin,
                IsActive              = pp.IsActive,
                AssignedAt            = pp.AssignedAt
            }).ToList();

            return ApiResponse<List<PersonProfessionalResponse>>.SuccessResult(response);
        }
    }
}
