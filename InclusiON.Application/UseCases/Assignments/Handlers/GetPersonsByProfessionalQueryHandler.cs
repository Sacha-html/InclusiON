using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class GetPersonsByProfessionalQueryHandler
        : IQueryHandler<GetPersonsByProfessionalQuery, ApiResponse<List<ProfessionalPersonResponse>>>
    {
        private readonly IAssignmentsRepository _repository;

        public GetPersonsByProfessionalQueryHandler(IAssignmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ProfessionalPersonResponse>>> HandleAsync(
            GetPersonsByProfessionalQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetPersonsByProfessionalIdAsync(query.ProfessionalId, cancellationToken);

            var response = assignments.Select(GetPersonsByProfessionalQuery.MapToResponse).ToList();
            return ApiResponse<List<ProfessionalPersonResponse>>.SuccessResult(response);
        }
    }
}
