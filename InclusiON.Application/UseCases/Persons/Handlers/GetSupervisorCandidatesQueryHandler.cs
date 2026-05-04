using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetSupervisorCandidatesQueryHandler
        : IQueryHandler<GetSupervisorCandidatesQuery, ApiResponse<List<SupervisorCandidateResponse>>>
    {
        private readonly IPersonsRepository _repository;

        public GetSupervisorCandidatesQueryHandler(IPersonsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<SupervisorCandidateResponse>>> HandleAsync(
            GetSupervisorCandidatesQuery query, CancellationToken cancellationToken)
        {
            var professionals = await _repository.GetSupervisingProfessionalsAsync(query.PersonId, cancellationToken);
            var representatives = await _repository.GetActiveRepresentativesAsync(query.PersonId, cancellationToken);

            var candidates = professionals.Select(p => new SupervisorCandidateResponse
                {
                    UserId = p.UserId,
                    FullName = $"{p.FirstName} {p.LastName}",
                    Type = RoleNames.Professional
                })
                .Concat(representatives.Select(pr => new SupervisorCandidateResponse
                {
                    UserId = pr.Representative.UserId,
                    FullName = $"{pr.Representative.FirstName} {pr.Representative.LastName}",
                    Type = RoleNames.Family,
                    Relationship = pr.Relationship
                }))
                .OrderBy(c => c.FullName)
                .ToList();

            return ApiResponse<List<SupervisorCandidateResponse>>.SuccessResult(candidates);
        }
    }
}
