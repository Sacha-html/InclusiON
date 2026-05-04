using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetAssignmentByIdQueryHandler
        : IQueryHandler<GetAssignmentByIdQuery, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;

        public GetAssignmentByIdQueryHandler(IActivityAssignmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            GetAssignmentByIdQuery query, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(query.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            // Solo puede acceder la persona asignada o el profesional que la asignó
            if (assignment.PersonId != query.RequesterId &&
                assignment.AssignedByProfessionalId != query.RequesterId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(
                ActivityAssignmentResponse.From(assignment));
        }
    }
}
