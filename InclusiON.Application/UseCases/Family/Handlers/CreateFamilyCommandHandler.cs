using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Family.Handlers
{
    public class CreateFamilyCommandHandler : ICommandHandler<CreateFamilyCommand, ApiResponse<FamilyResponse>>
    {
        private readonly IFamilyRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateFamilyCommandHandler> _logger;

        public CreateFamilyCommandHandler(
            IFamilyRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<CreateFamilyCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<FamilyResponse>> HandleAsync(CreateFamilyCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, null, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<FamilyResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            ErrorMessages.DocumentAlreadyExists);
                    }
                }

                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                var password = $"Temp@{Guid.NewGuid().ToString()[..8]}";

                var user = new User
                {
                    UserName = command.Email,
                    Email = command.Email,
                    Name = command.FirstName,
                    Surname = command.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    MustChangePassword = true
                };

                var family = new FamilyRepresentative
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DocumentNumber = command.DocumentNumber,
                    Phone = command.Phone,
                    Relationship = command.Relationship
                };

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var (succeeded, errors) = await _identityService.CreateUserAsync(user, password);
                    if (!succeeded)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UserCreationError, string.Join(", ", errors)));
                    }

                    await _identityService.AddToRoleAsync(user, "FamilyRepresentative");

                    family.UserId = user.Id;
                    await _repository.CreateAsync(family, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation("Familiar creado: {FamilyId}, Usuario: {UserId}", family.Id, user.Id);

                var response = GetFamilyByIdQueryHandler.MapToResponse(family);
                response.TemporaryPassword = password;
                response.Email = user.Email;
                return ApiResponse<FamilyResponse>.SuccessResult(response, "Familiar creado exitosamente");
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith(ErrorMessages.UserCreationError.Replace("{0}", "")))
            {
                _logger.LogWarning(ex, "Error de validacion al crear familiar");
                return ApiResponse<FamilyResponse>.ErrorResult(ErrorCode.ValidationFailed, ex.Message);
            }
        }
    }
}
