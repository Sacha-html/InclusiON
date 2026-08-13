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
    /// Manejador para dar de baja un aula.
    /// Desactiva el aula y desvincula a los alumnos de ella (quedan sin aula pero siguen asignados al profesional).
    /// </summary>
    public class DeactivateClassroomCommandHandler
        : ICommandHandler<DeactivateClassroomCommand, ApiResponse<ClassroomResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateClassroomCommandHandler(IAssignmentsRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ClassroomResponse>> HandleAsync(
            DeactivateClassroomCommand command, CancellationToken cancellationToken)
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

            if (!classroom.IsActive)
                return ApiResponse<ClassroomResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "El aula ya está dada de baja.");

            await _repository.DeactivateClassroomAsync(command.ClassroomId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<ClassroomResponse>.SuccessResult(
                ClassroomResponse.MapToResponse(classroom),
                "Aula dada de baja exitosamente. Los alumnos siguen asignados al profesional.");
        }
    }
}
