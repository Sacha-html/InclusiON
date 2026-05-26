using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Handlers
{
    public class PatchInstitutionStatusCommandHandler
        : ICommandHandler<PatchInstitutionStatusCommand, ApiResponse<InstitutionResponse>>
    {
        private readonly IInstitutionsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public PatchInstitutionStatusCommandHandler(
            IInstitutionsRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<InstitutionResponse>> HandleAsync(
            PatchInstitutionStatusCommand command, CancellationToken cancellationToken)
        {
            var institution = await _repository.GetByIdAsync(command.InstitutionId, cancellationToken);

            if (institution == null)
                return ApiResponse<InstitutionResponse>.NotFound("Institución educativa");

            // Máquina de estados: rechazar transiciones no-op
            if (institution.IsActive == command.IsActive)
            {
                var estado = command.IsActive ? "activa" : "inactiva";
                return ApiResponse<InstitutionResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    $"La institución ya se encuentra {estado}.");
            }

            // Transición activo → inactivo: validar integridad
            if (!command.IsActive)
            {
                var hasProfessionals = await _repository.HasActiveProfessionalsAsync(command.InstitutionId, cancellationToken);
                if (hasProfessionals)
                    return ApiResponse<InstitutionResponse>.Conflict(
                        ErrorCode.BusinessRuleViolation,
                        "No se puede dar de baja la institución porque tiene profesionales activos asignados. Reasigne o desactive los profesionales primero.");
            }

            institution.IsActive = command.IsActive;
            institution.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(institution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var mensaje = command.IsActive
                ? "Institución reactivada exitosamente."
                : "Institución dada de baja exitosamente.";

            var response = InstitutionResponse.MapToResponse(institution);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(institution.Id.ToString()));
            return ApiResponse<InstitutionResponse>.SuccessResult(response, mensaje);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
