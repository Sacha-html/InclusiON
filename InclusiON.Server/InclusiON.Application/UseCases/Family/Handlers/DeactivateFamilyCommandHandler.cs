using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class DeactivateFamilyCommandHandler : ICommandHandler<DeactivateFamilyCommand, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateFamilyCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public DeactivateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateFamilyCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(DeactivateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(command.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            // Desactivar relaciones y suspender alumnos que se queden sin representantes
            var suspendedStudents = await _repository.DeactivateRepresentativeAndSuspendDependentStudentsAsync(family.UserId, _dateTime.UtcNow, cancellationToken);
            suspendedStudents ??= new List<string>();

            if (family.User != null)
            {
                family.User.IsActive = false;
            }

            family.IsActive = false;
            family.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(family, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Familiar desactivado: {FamilyId}", family.Id);

            var response = FamilyResponse.MapToResponse(family);
            var successMessage = suspendedStudents.Count > 0
                ? $"Familiar desactivado exitosamente. Se ha suspendido el acceso de los alumnos: {string.Join(", ", suspendedStudents)} por no contar con otro representante activo."
                : "Familiar desactivado exitosamente";

            return ApiResponse<FamilyResponse>.SuccessResult(response, successMessage);
        }
    }
}
