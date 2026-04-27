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

        public CreateInstitutionCommandHandler(
            IInstitutionsRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
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
            return ApiResponse<InstitutionResponse>.SuccessResult(response, "Institucion creada exitosamente.");
        }
    }
}
