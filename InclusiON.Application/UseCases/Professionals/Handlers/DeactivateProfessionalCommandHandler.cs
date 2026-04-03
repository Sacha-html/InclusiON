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
    public class DeactivateProfessionalCommandHandler : ICommandHandler<DeactivateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateProfessionalCommandHandler> _logger;

        public DeactivateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateProfessionalCommandHandler> logger)
        {
            _repository = repository;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(DeactivateProfessionalCommand command, CancellationToken cancellationToken)
        {
            var professional = await _repository.GetByIdAsync(command.ProfessionalId, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            // Desactivar usuario
            professional.User.IsActive = false;

            // Revocar refresh tokens activos
            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                professional.UserId,
                Constants.RevokeReasons.ProfessionalDeactivated,
                cancellationToken);

            await _repository.UpdateAsync(professional, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profesional desactivado: {ProfessionalId}, Usuario: {UserId}", command.ProfessionalId, professional.UserId);

            var response = GetProfessionalByIdQuery.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalDeactivated);
        }
    }
}
