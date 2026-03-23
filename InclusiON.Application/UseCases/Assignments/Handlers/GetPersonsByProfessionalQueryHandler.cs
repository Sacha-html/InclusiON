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
            var person = assignment.Person;
            int? age = null;
            if (person != null && person.BirthDate != default)
            {
                var today = DateTime.UtcNow;
                age = today.Year - person.BirthDate.Year;
                if (person.BirthDate.Date > today.AddYears(-age.Value)) age--;
            }

            return new ProfessionalPersonResponse
            {
                ProfessionalId = assignment.ProfessionalId,
                PersonId = assignment.PersonId,
                PersonFirstName = person?.FirstName ?? string.Empty,
                PersonLastName = person?.LastName ?? string.Empty,
                AvatarColor = person?.AvatarColor,
                DisabilityTypeName = person?.DisabilityType?.Name,
                Age = age,
                AssignedAt = assignment.AssignedAt,
                IsPrimaryProfessional = assignment.IsPrimaryProfessional,
                CanSuperviseLogin = assignment.CanSuperviseLogin,
                IsActive = assignment.IsActive
            };
        }
    }
}
