using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
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

        public DeactivateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateFamilyCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(DeactivateFamilyCommand command, CancellationToken cancellationToken)
        {
            var family = await _repository.GetByIdAsync(command.FamilyId, cancellationToken);

            if (family == null)
            {
                return ApiResponse<FamilyResponse>.NotFound("Familiar");
            }

            if (family.User != null)
            {
                family.User.IsActive = false;
            }

            family.IsActive = false;
            family.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Familiar desactivado: {FamilyId}", family.Id);

            var response = GetFamilyByIdQueryHandler.MapToResponse(family);
            return ApiResponse<FamilyResponse>.SuccessResult(response, "Familiar desactivado exitosamente");
        }
    }
}
