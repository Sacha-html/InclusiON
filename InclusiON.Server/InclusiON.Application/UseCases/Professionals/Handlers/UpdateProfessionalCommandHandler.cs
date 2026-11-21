using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class UpdateProfessionalCommandHandler : ICommandHandler<UpdateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IReadOnlyRepository<Specialty>? _specialties;

        public UpdateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IUniquenessValidationService uniquenessValidation,
            IUnitOfWork unitOfWork,
            ILogger<UpdateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime,
            IReadOnlyRepository<Specialty>? specialties = null)
        {
            _repository = repository;
            _uniquenessValidation = uniquenessValidation;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _specialties = specialties;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(UpdateProfessionalCommand command, CancellationToken cancellationToken)
        {
            var professional = await _repository.GetByIdAsync(command.ProfessionalId, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            var normalizedDoc = UniquenessNormalizer.NormalizeDocument(command.DocumentNumber);
            var normalizedLicense = UniquenessNormalizer.NormalizeLicense(command.LicenseNumber);

            // Validar documento unico global si cambio
            if (normalizedDoc is not null && !string.Equals(normalizedDoc, professional.DocumentNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var docCheck = await _uniquenessValidation.CheckDocumentAsync(normalizedDoc, command.ProfessionalId, cancellationToken);
                if (!docCheck.IsAvailable)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.DocumentAlreadyExists,
                        docCheck.Message ?? ErrorMessages.DocumentAlreadyExists);
                }
            }

            // Validar matricula unica si cambio
            if (normalizedLicense is not null && !string.Equals(normalizedLicense, professional.LicenseNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var licenseCheck = await _uniquenessValidation.CheckLicenseNumberAsync(normalizedLicense, command.ProfessionalId, cancellationToken);
                if (!licenseCheck.IsAvailable)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.LicenseNumberAlreadyExists,
                        licenseCheck.Message ?? "Esta matrícula ya se encuentra registrada.");
                }
            }

            // Actualizar campos solo si se proporcionan
            if (command.FirstName != null) professional.FirstName = command.FirstName.Trim();
            if (command.LastName != null) professional.LastName = command.LastName.Trim();
            if (command.DocumentNumber != null) professional.DocumentNumber = normalizedDoc;
            if (command.Phone != null) professional.Phone = command.Phone.Trim();
            if (command.Specialty != null) professional.Specialty = command.Specialty;
            if (command.SpecialtyId.HasValue)
            {
                professional.SpecialtyId = command.SpecialtyId;
                professional.Specialty = null;
            }

            if (command.SpecialtyId.HasValue && _specialties is not null && (await _specialties.GetByIdAsync(command.SpecialtyId.Value, cancellationToken))?.IsActive != true)
                return ApiResponse<ProfessionalResponse>.ErrorResult(ErrorCode.ValidationFailed, "La especialidad seleccionada no está activa.");
            if (command.LicenseNumber != null) professional.LicenseNumber = normalizedLicense;
            if (command.BirthDate.HasValue) professional.BirthDate = command.BirthDate.Value;

            // Actualizar instituciones si se proporcionaron
            if (command.InstitutionIds != null)
            {
                // Desactivar las que ya no estan en la lista
                foreach (var pi in professional.ProfessionalInstitutions.ToList())
                {
                    if (!command.InstitutionIds.Contains(pi.InstitutionId))
                    {
                        pi.IsActive = false;
                    }
                }

                // Agregar las nuevas
                var existingIds = professional.ProfessionalInstitutions
                    .Where(pi => command.InstitutionIds.Contains(pi.InstitutionId))
                    .Select(pi => pi.InstitutionId)
                    .ToHashSet();

                foreach (var instId in command.InstitutionIds)
                {
                    if (!existingIds.Contains(instId))
                    {
                        professional.ProfessionalInstitutions.Add(new ProfessionalInstitution
                        {
                            ProfessionalId = professional.Id,
                            InstitutionId = instId,
                            AssignedAt = _dateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }
            }

            await _repository.UpdateAsync(professional, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profesional actualizado: {ProfessionalId}", command.ProfessionalId);

            var response = ProfessionalResponse.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalUpdated);
        }
    }
}
