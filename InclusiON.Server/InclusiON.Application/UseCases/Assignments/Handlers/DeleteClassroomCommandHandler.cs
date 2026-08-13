using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    /// <summary>
    /// Manejador para eliminar permanentemente un aula.
    /// Solo se permite si el aula no tiene alumnos activos asignados.
    /// </summary>
    public class DeleteClassroomCommandHandler
        : ICommandHandler<DeleteClassroomCommand, ApiResponse<ClassroomResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClassroomCommandHandler(IAssignmentsRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ClassroomResponse>> HandleAsync(
            DeleteClassroomCommand command, CancellationToken cancellationToken)
        {
            var classroom = await _repository.GetClassroomByIdAsync(command.ClassroomId, cancellationToken);
            if (classroom == null)
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.NotFound,
                    "El aula no existe.");

            if (classroom.ProfessionalId != command.ProfessionalId)
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.Forbidden,
                    "El aula no pertenece a este profesional.");

            var (success, error) = await _repository.DeleteClassroomAsync(command.ClassroomId, cancellationToken);

            if (!success)
            {
                if (error == "has_students")
                    return ApiResponse<ClassroomResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "No se puede eliminar el aula porque tiene alumnos activos asignados. Primero dé de baja el aula.");

                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.NotFound,
                    "El aula no existe.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<ClassroomResponse>.SuccessResult(
                ClassroomResponse.MapToResponse(classroom),
                "Aula eliminada exitosamente.");
        }
    }
}
