using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
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

        public UpdateInstitutionCommandHandler(
            IInstitutionsRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
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
            institution.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = GetInstitutionsQueryHandler.MapToResponse(institution);
            return ApiResponse<InstitutionResponse>.SuccessResult(response, "Institucion actualizada exitosamente.");
        }
    }
}
