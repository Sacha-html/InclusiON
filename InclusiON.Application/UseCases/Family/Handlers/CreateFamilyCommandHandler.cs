using Microsoft.Extensions.Logging;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Commands;
using InclusiON.Application.UseCases.Family.Queries;
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
        private readonly IPersonsRepository _personsRepository;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateFamilyCommandHandler> _logger;

        public CreateFamilyCommandHandler(
            IFamilyRepository repository,
            IPersonsRepository personsRepository,
            IIdentityService identityService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<CreateFamilyCommandHandler> logger)
        {
            _repository = repository;
            _personsRepository = personsRepository;
            _identityService = identityService;
            _emailService = emailService;
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

                var person = await _personsRepository.GetByIdAsync(command.PersonId, cancellationToken);
                if (person is null)
                {
                    return ApiResponse<FamilyResponse>.ErrorResult(
                        ErrorCode.PersonNotFound,
                        ErrorMessages.PersonNotFound);
                }

                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<FamilyResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                var password = PasswordGenerator.GenerateTemporary();

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

                    // Vincular familiar con la persona
                    family.PersonRepresentatives.Add(new PersonRepresentative
                    {
                        PersonId = command.PersonId,
                        RepresentativeId = family.Id,
                        IsPrimary = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation("Familiar creado: {FamilyId}, Usuario: {UserId}", family.Id, user.Id);

                // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
                // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
                // Enviar email con contraseña temporal
                try
                {
                    await _emailService.SendTemplatedEmailAsync(
                        command.Email,
                        "Bienvenido a InclusiON — Tu cuenta ha sido creada",
                        "PasswordReset",
                        new Dictionary<string, string?>
                        {
                            { "UserName", command.FirstName },
                            { "TemporaryPassword", password },
                            { "Year", DateTime.UtcNow.Year.ToString() }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar email de bienvenida a {Email}", command.Email);
                }

                var response = FamilyResponse.MapToResponse(family);
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
