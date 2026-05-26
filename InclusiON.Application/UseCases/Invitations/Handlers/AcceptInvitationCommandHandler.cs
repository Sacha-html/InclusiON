using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class AcceptInvitationCommandHandler : ICommandHandler<AcceptInvitationCommand, ApiResponse<AcceptInvitationResponse>>
    {
        private readonly IInvitationsRepository _invitationsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AcceptInvitationCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public AcceptInvitationCommandHandler(
            IInvitationsRepository invitationsRepository,
            IProfessionalsRepository professionalsRepository,
            IBackgroundJobRepository backgroundJobs,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<AcceptInvitationCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _invitationsRepository   = invitationsRepository;
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _identityService         = identityService;
            _unitOfWork              = unitOfWork;
            _logger                  = logger;
            _dateTime                = dateTime;
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

                if (invitation.ExpiresAt < _dateTime.UtcNow)
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
                    CreatedAt = _dateTime.UtcNow,
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
                    await _identityService.AddToRoleAsync(user, RoleNames.FamilyRepresentative);

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
                            CreatedAt = _dateTime.UtcNow
                        };
                        await _invitationsRepository.CreatePersonRepresentativeAsync(personRep, ct);
                    }

                    // Marcar invitacion como usada
                    invitation.IsUsed = true;
                    invitation.UsedAt = _dateTime.UtcNow;
                    invitation.UsedByUserId = user.Id;
                    await _invitationsRepository.UpdateAsync(invitation, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation(
                    "Invitacion {InvitationId} aceptada. Usuario: {UserId}, Familiar: {FamilyRepId}",
                    invitation.Id, user.Id, familyRep?.Id);

                // Notificar al profesional que creó la invitación — fire and forget
                var invFirstName = invitation.FirstName ?? string.Empty;
                var invLastName  = invitation.LastName  ?? string.Empty;
                var createdById  = invitation.CreatedByProfessionalId;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var prof = await _professionalsRepository.GetByIdAsync(createdById, CancellationToken.None);
                        if (prof is not null)
                        {
                            await _backgroundJobs.CreateAsync(
                                JobTypes.Push,
                                JsonSerializer.Serialize(new NotificationPayload
                                {
                                    UserId    = prof.UserId.ToString(),
                                    Title     = "Invitación aceptada",
                                    Message   = $"{invFirstName} {invLastName} aceptó tu invitación y se unió como familiar.",
                                    ActionUrl = "/#/pro/family"
                                }),
                                maxRetries: 3);
                        }
                    }
                    catch { /* fire and forget */ }
                });

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
                    "No se pudo crear el usuario. Verificá que los datos ingresados sean válidos.");
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
