using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Assignments;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Assignments.Handlers
{
    public class AssignPersonCommandHandler
        : ICommandHandler<AssignPersonCommand, ApiResponse<ProfessionalPersonResponse>>
    {
        private readonly IAssignmentsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignPersonCommandHandler(
            IAssignmentsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ProfessionalPersonResponse>> HandleAsync(
            AssignPersonCommand command, CancellationToken cancellationToken)
        {
            // Validar que el profesional existe
            var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
            if (professional == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            // Validar que la persona existe
            var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (person == null)
            {
                return ApiResponse<ProfessionalPersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Validar que no exista una asignacion activa
            var existing = await _repository.GetAssignmentAsync(command.ProfessionalId, command.PersonId, cancellationToken);
            if (existing != null && existing.IsActive)
            {
                return ApiResponse<ProfessionalPersonResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    "La asignacion profesional-persona ya existe y esta activa.");
            }

            // Si existe pero esta inactiva, reactivar
            if (existing != null && !existing.IsActive)
            {
                existing.IsActive = true;
                existing.IsPrimaryProfessional = command.IsPrimaryProfessional;
                existing.CanSuperviseLogin = command.CanSuperviseLogin;
                existing.AssignedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                existing.Person = person;
                var reactivatedResponse = GetPersonsByProfessionalQueryHandler.MapToResponse(existing);
                return ApiResponse<ProfessionalPersonResponse>.SuccessResult(reactivatedResponse, "Asignacion reactivada exitosamente.");
            }

            var assignment = new ProfessionalPerson
            {
                ProfessionalId = command.ProfessionalId,
                PersonId = command.PersonId,
                IsPrimaryProfessional = command.IsPrimaryProfessional,
                CanSuperviseLogin = command.CanSuperviseLogin,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.CreateAssignmentAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            assignment.Person = person;
            var response = GetPersonsByProfessionalQueryHandler.MapToResponse(assignment);
            return ApiResponse<ProfessionalPersonResponse>.SuccessResult(response, "Persona asignada al profesional exitosamente.");
        }
    }
}
