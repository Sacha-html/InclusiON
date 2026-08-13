using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    /// <summary>
    /// Manejador de la consulta para obtener las aulas de un profesional.
    /// </summary>
    public class GetClassroomsByProfessionalQueryHandler
        : IQueryHandler<GetClassroomsByProfessionalQuery, ApiResponse<List<ClassroomResponse>>>
    {
        private readonly IAssignmentsRepository _repository;

        public GetClassroomsByProfessionalQueryHandler(IAssignmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<List<ClassroomResponse>>> HandleAsync(
            GetClassroomsByProfessionalQuery query, CancellationToken cancellationToken)
        {
            var classrooms = await _repository.GetClassroomsByProfessionalIdAsync(query.ProfessionalId, cancellationToken);

            var response = classrooms.Select(ClassroomResponse.MapToResponse).ToList();
            return ApiResponse<List<ClassroomResponse>>.SuccessResult(response);
        }
    }
}
