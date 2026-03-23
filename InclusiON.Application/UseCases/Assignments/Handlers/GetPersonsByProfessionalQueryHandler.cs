using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.Domain.Models;
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

            var response = assignments.Select(MapToResponse).ToList();
            return ApiResponse<List<ProfessionalPersonResponse>>.SuccessResult(response);
        }

        internal static ProfessionalPersonResponse MapToResponse(ProfessionalPerson assignment)
        {
            return new ProfessionalPersonResponse
            {
                ProfessionalId = assignment.ProfessionalId,
                PersonId = assignment.PersonId,
                PersonFirstName = assignment.Person?.FirstName ?? string.Empty,
                PersonLastName = assignment.Person?.LastName ?? string.Empty,
                AssignedAt = assignment.AssignedAt,
                IsPrimaryProfessional = assignment.IsPrimaryProfessional,
                CanSuperviseLogin = assignment.CanSuperviseLogin,
                IsActive = assignment.IsActive
            };
        }
    }
}
