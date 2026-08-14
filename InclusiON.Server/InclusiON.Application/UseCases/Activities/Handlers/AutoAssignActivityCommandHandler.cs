using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class AutoAssignActivityCommandHandler
        : ICommandHandler<AutoAssignActivityCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public AutoAssignActivityCommandHandler(
            IActivityAssignmentRepository repository,
            IActivitiesRepository activitiesRepository,
            IPersonsRepository personsRepository,
            IProfessionalsRepository professionalsRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _activitiesRepository = activitiesRepository;
            _personsRepository = personsRepository;
            _professionalsRepository = professionalsRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            AutoAssignActivityCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar si ya existe una asignación para esta persona y actividad (no cancelada)
            var existingAssignments = await _repository.GetByPersonIdAsync(command.PersonId, cancellationToken);
            var existing = existingAssignments.FirstOrDefault(a => a.ActivityId == command.ActivityId && a.StatusId != AssignmentStatuses.Cancelada);
            if (existing is not null)
            {
                var existingDto = ActivityAssignmentResponse.From(existing);
                existingDto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(existing.Id.ToString()));
                foreach (var resp in existingDto.Responses)
                    resp.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(resp.Id.ToString()));
                return ApiResponse<ActivityAssignmentResponse>.SuccessResult(existingDto);
            }

            // 2. Obtener actividad
            var activity = await _activitiesRepository.GetByIdAsync(command.ActivityId, cancellationToken);
            if (activity is null || !activity.IsActive)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Actividad");

            // 3. Buscar profesional asignado a la persona o el primer profesional activo del sistema
            var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Persona");

            Guid profId = Guid.Empty;
            if (person.ProfessionalPersons != null && person.ProfessionalPersons.Count > 0)
            {
                profId = person.ProfessionalPersons.First().ProfessionalId;
            }
            else
            {
                var pagedProf = await _professionalsRepository.GetPagedAsync(
                    page: 1,
                    pageSize: 1,
                    search: null,
                    specialty: null,
                    isActive: true,
                    status: null,
                    sortBy: null,
                    sortDirection: "ASC",
                    institutionIds: null,
                    cancellationToken: cancellationToken);

                if (pagedProf.Data.Count > 0)
                {
                    profId = pagedProf.Data[0].Id;
                }
            }

            // 4. Crear asignación
            var assignment = new ActivityAssignment
            {
                ActivityId               = activity.Id,
                PersonId                 = command.PersonId,
                AssignedByProfessionalId = profId,
                AssignedAt               = _dateTime.UtcNow,
                DueDate                  = null,
                StatusId                 = AssignmentStatuses.Pendiente,
                IsEvaluationActivity     = false,
                SequenceOrder            = activity.RoadmapOrder,
                CreatedAt                = _dateTime.UtcNow,
            };

            await _repository.CreateAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Cargar asignación completa para mapear DTO
            var created = await _repository.GetByIdAsync(assignment.Id, cancellationToken);
            var dto = ActivityAssignmentResponse.From(created ?? assignment);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(assignment.Id.ToString()));
            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
