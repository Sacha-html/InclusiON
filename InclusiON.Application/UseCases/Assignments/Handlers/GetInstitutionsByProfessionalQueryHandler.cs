using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class GetInstitutionsByProfessionalQueryHandler
        : IQueryHandler<GetInstitutionsByProfessionalQuery, ApiResponse<List<ProfessionalInstitutionResponse>>>
    {
        private readonly IAssignmentsRepository _repository;

        public GetInstitutionsByProfessionalQueryHandler(IAssignmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ProfessionalInstitutionResponse>>> HandleAsync(
            GetInstitutionsByProfessionalQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetInstitutionsByProfessionalIdAsync(query.ProfessionalId, cancellationToken);

            var response = assignments.Select(ProfessionalInstitutionResponse.MapToResponse).ToList();
            return ApiResponse<List<ProfessionalInstitutionResponse>>.SuccessResult(response);
        }
    }
}
