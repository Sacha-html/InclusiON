using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class UpdateProfessionalCommandHandler : ICommandHandler<UpdateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProfessionalCommandHandler> _logger;

        public UpdateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateProfessionalCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
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

            // Validar documento unico si cambio
            if (!string.IsNullOrWhiteSpace(command.DocumentNumber) && command.DocumentNumber != professional.DocumentNumber)
            {
                var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, command.ProfessionalId, cancellationToken);
                if (documentExists)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.DocumentAlreadyExists,
                        ErrorMessages.DocumentAlreadyExists);
                }
            }

            // Actualizar campos solo si se proporcionan
            if (command.FirstName != null) professional.FirstName = command.FirstName;
            if (command.LastName != null) professional.LastName = command.LastName;
            if (command.DocumentNumber != null) professional.DocumentNumber = command.DocumentNumber;
            if (command.Phone != null) professional.Phone = command.Phone;
            if (command.Specialty != null) professional.Specialty = command.Specialty;
            if (command.LicenseNumber != null) professional.LicenseNumber = command.LicenseNumber;
            if (command.BirthDate.HasValue) professional.BirthDate = command.BirthDate;

            await _repository.UpdateAsync(professional, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profesional actualizado: {ProfessionalId}", command.ProfessionalId);

            var response = GetProfessionalByIdQuery.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalUpdated);
        }
    }
}
