using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetPersonActivityAssignmentsQueryHandler
        : IQueryHandler<GetPersonActivityAssignmentsQuery, ApiResponse<List<ActivityAssignmentResponse>>>
    {
        private readonly IActivityAssignmentRepository _repository;

        public GetPersonActivityAssignmentsQueryHandler(IActivityAssignmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ActivityAssignmentResponse>>> HandleAsync(
            GetPersonActivityAssignmentsQuery query, CancellationToken cancellationToken)
        {
            var assignments = await _repository.GetByPersonIdAsync(query.PersonId, cancellationToken);

            var response = assignments
                .Select(ActivityAssignmentResponse.From)
                .ToList();

            return ApiResponse<List<ActivityAssignmentResponse>>.SuccessResult(response);
        }
    }
}
