using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.Application.UseCases.Institutions.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Institutions;

namespace InclusiON.Application.UseCases.Institutions.Handlers
{
    public class CreateInstitutionCommandHandler
        : ICommandHandler<CreateInstitutionCommand, ApiResponse<InstitutionResponse>>
    {
        private readonly IInstitutionsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public CreateInstitutionCommandHandler(
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
            CreateInstitutionCommand command, CancellationToken cancellationToken)
        {
            // Validar nombre unico
            var exists = await _repository.ExistsByNameAsync(command.Name, null, cancellationToken);
            if (exists)
            {
                return ApiResponse<InstitutionResponse>.Conflict(
                    ErrorCode.DuplicateEntry,
                    "Ya existe una institucion con ese nombre.");
            }

            var institution = new EducationalInstitution
            {
                Name = command.Name,
                Address = command.Address,
                Phone = command.Phone,
                Email = command.Email,
                IsActive = true,
                CreatedAt = _dateTime.UtcNow
            };

            await _repository.CreateAsync(institution, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = InstitutionResponse.MapToResponse(institution);
            response.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(institution.Id.ToString()));
            return ApiResponse<InstitutionResponse>.SuccessResult(response, "Institucion creada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
