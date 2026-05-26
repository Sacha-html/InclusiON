using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class RemoveAdminInstitutionCommandHandler
        : ICommandHandler<RemoveAdminInstitutionCommand, ApiResponse<AdminInstitutionResponse>>
    {
        private readonly IAdminInstitutionRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryption;

        public RemoveAdminInstitutionCommandHandler(IAdminInstitutionRepository repository, IUnitOfWork unitOfWork, IEncryptionService encryption)
        {
            _repository  = repository;
            _unitOfWork  = unitOfWork;
            _encryption  = encryption;
        }

        public async Task<ApiResponse<AdminInstitutionResponse>> HandleAsync(
            RemoveAdminInstitutionCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.FindAssignmentAsync(
                command.AdminUserId, command.InstitutionId, cancellationToken);

            if (assignment is null)
                return ApiResponse<AdminInstitutionResponse>.NotFound("Asignación");

            _repository.Remove(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<AdminInstitutionResponse>.SuccessResult(new AdminInstitutionResponse
            {
                AdminUserId             = assignment.AdminUserId,
                InstitutionId           = assignment.InstitutionId,
                EncryptedInstitutionId  = _encryption.EncryptId(assignment.InstitutionId),
                InstitutionName         = assignment.Institution.Name,
                AssignedAt              = assignment.AssignedAt,
                IsActive                = assignment.IsActive
            }, "Asignación eliminada exitosamente.");
        }

    }
}
