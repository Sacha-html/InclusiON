using InclusiON.Application.Extensions;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class AssignInstitutionToAdminCommandHandler
        : ICommandHandler<AssignInstitutionToAdminCommand, ApiResponse<AdminInstitutionResponse>>
    {
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;
        private readonly IInstitutionsRepository _institutionsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public AssignInstitutionToAdminCommandHandler(
            IAdminInstitutionRepository adminInstitutionRepository,
            IInstitutionsRepository institutionsRepository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _adminInstitutionRepository = adminInstitutionRepository;
            _institutionsRepository     = institutionsRepository;
            _identityService            = identityService;
            _unitOfWork                 = unitOfWork;
            _dateTime                   = dateTime;
            _encryption                 = encryption;
        }

        public async Task<ApiResponse<AdminInstitutionResponse>> HandleAsync(
            AssignInstitutionToAdminCommand command, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(command.AdminUserId);
            if (user is null)
                return ApiResponse<AdminInstitutionResponse>.NotFound("Usuario");

            var institution = await _institutionsRepository.GetByIdAsync(command.InstitutionId, cancellationToken);
            if (institution is null)
                return ApiResponse<AdminInstitutionResponse>.NotFound("Institución");

            var existing = await _adminInstitutionRepository
                .FindAssignmentAsync(command.AdminUserId, command.InstitutionId, cancellationToken);

            if (existing is not null)
            {
                if (!existing.IsActive)
                {
                    existing.IsActive   = true;
                    existing.AssignedAt = _dateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var existingResponse = AdminInstitutionMapper.ToResponse(existing, institution.Name);
                existingResponse.EncryptedInstitutionId = _encryption.EncryptId(existing.InstitutionId);
                return ApiResponse<AdminInstitutionResponse>.SuccessResult(existingResponse, "Asignación creada exitosamente.");
            }

            var assignment = new AdminInstitution
            {
                AdminUserId   = command.AdminUserId,
                InstitutionId = command.InstitutionId,
                AssignedAt    = _dateTime.UtcNow,
                IsActive      = true
            };

            await _adminInstitutionRepository.AddAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = AdminInstitutionMapper.ToResponse(assignment, institution.Name);
            response.EncryptedInstitutionId = _encryption.EncryptId(assignment.InstitutionId);
            return ApiResponse<AdminInstitutionResponse>.SuccessResult(response, "Asignación creada exitosamente.");
        }

    }
}
