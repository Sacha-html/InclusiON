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
    public class CreateInvitationCommandHandler : ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateInvitationCommandHandler> _logger;

        public CreateInvitationCommandHandler(
            IInvitationsRepository repository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<CreateInvitationCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<InvitationResponse>> HandleAsync(CreateInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que el email no este ya registrado
                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<InvitationResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                var invitation = new Invitation
                {
                    CreatedByProfessionalId = command.ProfessionalId,
                    ForPersonId = command.PersonId,
                    Code = Guid.NewGuid().ToString(),
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    Relationship = command.Relationship,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsUsed = false
                };

                await _repository.CreateAsync(invitation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Invitacion creada: {InvitationId}, Email: {Email}", invitation.Id, invitation.Email);

                // Enviar email con el link de invitacion (fire-and-forget, no bloquea)
                if (!string.IsNullOrEmpty(command.BaseUrl))
                {
                    _ = SendInvitationEmailAsync(invitation, command.BaseUrl, cancellationToken);
                }

                var response = MapToResponse(invitation);
                return ApiResponse<InvitationResponse>.SuccessResult(response, SuccessMessages.InvitationCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear invitacion para {Email}", command.Email);
                return ApiResponse<InvitationResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorCreateInvitation);
            }
        }

        private async Task SendInvitationEmailAsync(Invitation invitation, string baseUrl, CancellationToken cancellationToken)
        {
            try
            {
                var inviteUrl = $"{baseUrl.TrimEnd('/')}/#/invite/{invitation.Code}";

                var replacements = new Dictionary<string, string?>
                {
                    ["RecipientName"] = !string.IsNullOrEmpty(invitation.FirstName) ? invitation.FirstName : "Familiar",
                    ["InviteUrl"] = inviteUrl,
                    ["ExpiresAt"] = invitation.ExpiresAt.ToString("dd/MM/yyyy HH:mm"),
                    ["PersonName"] = invitation.ForPerson != null
                        ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                        : null,
                    ["Relationship"] = invitation.Relationship,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                };

                await _emailService.SendTemplatedEmailAsync(
                    invitation.Email,
                    "InclusiON - Invitacion para registro familiar",
                    "invitation",
                    replacements,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar email de invitacion a {Email}", invitation.Email);
            }
        }

        internal static InvitationResponse MapToResponse(Invitation invitation)
        {
            var now = DateTime.UtcNow;
            string status;

            if (invitation.IsUsed)
                status = "Aceptada";
            else if (invitation.ExpiresAt < now)
                status = "Expirada";
            else
                status = "Enviada";

            return new InvitationResponse
            {
                Id = invitation.Id,
                Code = invitation.Code,
                Email = invitation.Email,
                FirstName = invitation.FirstName,
                LastName = invitation.LastName,
                Relationship = invitation.Relationship,
                PersonName = invitation.ForPerson != null
                    ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                    : null,
                ExpiresAt = invitation.ExpiresAt,
                IsUsed = invitation.IsUsed,
                UsedAt = invitation.UsedAt,
                Status = status,
                CreatedByProfessionalName = invitation.CreatedByProfessional != null
                    ? $"{invitation.CreatedByProfessional.FirstName} {invitation.CreatedByProfessional.LastName}".Trim()
                    : null,
                CreatedAt = invitation.CreatedAt
            };
        }
    }
}
