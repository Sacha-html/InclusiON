using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Handlers
{
    public class UpdateInstitutionCommandHandler
        : ICommandHandler<UpdateInstitutionCommand, ApiResponse<InstitutionResponse>>
    {
        private readonly IInstitutionsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public UpdateInstitutionCommandHandler(
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
            UpdateInstitutionCommand command, CancellationToken cancellationToken)
        {
            var institution = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (institution == null)
            {
                return ApiResponse<InstitutionResponse>.NotFound("Institucion educativa");
            }

            // Validar nombre unico (excluyendo la misma institucion)
            var exists = await _repository.ExistsByNameAsync(command.Name, command.Id, cancellationToken);
            if (exists)
            {
                return ApiResponse<InstitutionResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    "Ya existe otra institucion con ese nombre.");
            }

            institution.Name = command.Name;
            institution.Address = command.Address;
            institution.Phone = command.Phone;
            institution.Email = command.Email;
            institution.UpdatedAt = _dateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = InstitutionResponse.MapToResponse(institution);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(institution.Id.ToString()));
            return ApiResponse<InstitutionResponse>.SuccessResult(response, "Institucion actualizada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
