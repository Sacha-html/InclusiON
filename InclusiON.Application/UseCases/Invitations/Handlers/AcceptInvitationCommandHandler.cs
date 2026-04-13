using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, ApiResponse<AcceptInvitationResponse>>
    {
        private readonly IInvitationsRepository _invitationsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AcceptInvitationCommandHandler> _logger;

        public AcceptInvitationCommandHandler(
            IInvitationsRepository invitationsRepository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<AcceptInvitationCommandHandler> logger)
        {
            _invitationsRepository = invitationsRepository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<AcceptInvitationResponse>> HandleAsync(AcceptInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validar invitacion
                var invitation = await _invitationsRepository.GetByCodeAsync(command.Code, cancellationToken);

                if (invitation == null)
                {
                    return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                        ErrorCode.InvitationNotFound,
                        ErrorMessages.InvitationNotFound);
                }

                if (invitation.IsUsed)
                {
                    return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                        ErrorCode.InvitationAlreadyUsed,
                        ErrorMessages.InvitationAlreadyUsed);
                }

                if (invitation.ExpiresAt < DateTime.UtcNow)
                {
                    return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                        ErrorCode.InvitationExpired,
                        ErrorMessages.InvitationExpired);
                }

                // Validar contraseñs
                if (command.Password != command.ConfirmPassword)
                {
                    return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        ErrorMessages.PasswordsMismatch);
                }

                // Validar email unico
                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AcceptInvitationResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                // Crear usuario + familiar + relacion en transaccion
                var user = new User
                {
                    UserName = command.Email,
                    Email = command.Email,
                    Name = invitation.FirstName ?? string.Empty,
                    Surname = invitation.LastName ?? string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    MustChangePassword = false
                };

                FamilyRepresentative? familyRep = null;

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    // Crear usuario
                    var (succeeded, errors) = await _identityService.CreateUserAsync(user, command.Password);
                    if (!succeeded)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UserCreationError, string.Join(", ", errors)));
                    }

                    // Asignar rol FamilyRepresentative
                    await _identityService.AddToRoleAsync(user, "FamilyRepresentative");

                    // Crear FamilyRepresentative
                    familyRep = new FamilyRepresentative
                    {
                        UserId = user.Id,
                        FirstName = invitation.FirstName ?? string.Empty,
                        LastName = invitation.LastName ?? string.Empty,
                        Relationship = invitation.Relationship
                    };
                    await _invitationsRepository.CreateFamilyRepresentativeAsync(familyRep, ct);

                    // Crear PersonRepresentative si hay persona asociada
                    if (invitation.ForPersonId.HasValue)
                    {
                        var personRep = new PersonRepresentative
                        {
                            PersonId = invitation.ForPersonId.Value,
                            RepresentativeId = familyRep.Id,
                            IsPrimary = true,
                            HasInformedConsent = false,
                            CanSuperviseLogin = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _invitationsRepository.CreatePersonRepresentativeAsync(personRep, ct);
                    }

                    // Marcar invitacion como usada
                    invitation.IsUsed = true;
                    invitation.UsedAt = DateTime.UtcNow;
                    invitation.UsedByUserId = user.Id;
                    await _invitationsRepository.UpdateAsync(invitation, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation(
                    "Invitacion {InvitationId} aceptada. Usuario: {UserId}, Familiar: {FamilyRepId}",
                    invitation.Id, user.Id, familyRep?.Id);

                var response = new AcceptInvitationResponse
                {
                    Success = true,
                    Message = SuccessMessages.InvitationAccepted
                };

                return ApiResponse<AcceptInvitationResponse>.SuccessResult(response, SuccessMessages.InvitationAccepted);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith(ErrorMessages.UserCreationError.Replace("{0}", "")))
            {
                _logger.LogWarning(ex, "Error de validacion al aceptar invitacion");
                return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aceptar invitacion {Code}", command.Code);
                return ApiResponse<AcceptInvitationResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorAcceptInvitation);
            }
        }
    }
}
