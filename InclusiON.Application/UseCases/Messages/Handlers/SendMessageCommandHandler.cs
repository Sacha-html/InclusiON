using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Application.UseCases.Messages.Handlers
{
    public class SendMessageCommandHandler
        : ICommandHandler<SendMessageCommand, ApiResponse<MessageResponse>>
    {
        private readonly IMessagesRepository    _messages;
        private readonly IUsersRepository       _users;
        private readonly IAssignmentsRepository _assignments;
        private readonly IUnitOfWork            _uow;

        public SendMessageCommandHandler(
            IMessagesRepository messages,
            IUsersRepository users,
            IAssignmentsRepository assignments,
            IUnitOfWork uow)
        {
            _messages    = messages;
            _users       = users;
            _assignments = assignments;
            _uow         = uow;
        }

        public async Task<ApiResponse<MessageResponse>> HandleAsync(
            SendMessageCommand command, CancellationToken cancellationToken)
        {
            // 1. Validar contenido no vacío
            if (string.IsNullOrWhiteSpace(command.Content))
                return ApiResponse<MessageResponse>.ErrorResult(
                    ErrorCode.InvalidInput, "El contenido del mensaje no puede estar vacío.");

            // 2. No se puede enviar un mensaje a sí mismo
            if (command.SenderId == command.ReceiverId)
                return ApiResponse<MessageResponse>.ErrorResult(
                    ErrorCode.InvalidInput, "No puedes enviarte un mensaje a ti mismo.");

            // 3. Cargar perfiles para determinar tipos de usuario
            var sender   = await _users.GetByIdWithProfileAsync(command.SenderId, cancellationToken);
            var receiver = await _users.GetByIdWithProfileAsync(command.ReceiverId, cancellationToken);

            if (sender is null)
                return ApiResponse<MessageResponse>.NotFound("Remitente");

            if (receiver is null || !receiver.IsActive)
                return ApiResponse<MessageResponse>.NotFound("Destinatario");

            // 4. Las personas con discapacidad no participan en mensajería
            if (sender.PersonWithDisability is not null)
                return ApiResponse<MessageResponse>.Forbidden(
                    "Las personas con discapacidad no pueden enviar mensajes desde este canal.");

            if (receiver.PersonWithDisability is not null)
                return ApiResponse<MessageResponse>.Forbidden(
                    "No se puede enviar mensajes a una persona con discapacidad.");

            // 5. Determinar tipos y validar relación
            var senderIsProfessional = sender.Professional is not null;
            var receiverIsProfessional = receiver.Professional is not null;

            // Mismo tipo: no permitido (prof→prof o familiar→familiar)
            if (senderIsProfessional == receiverIsProfessional)
                return ApiResponse<MessageResponse>.Forbidden(
                    "Solo se permiten mensajes entre profesionales y familiares vinculados a la misma persona.");

            // 6. Verificar que comparten al menos una persona activa
            var professionalUserId = senderIsProfessional ? command.SenderId : command.ReceiverId;
            var familyUserId       = senderIsProfessional ? command.ReceiverId : command.SenderId;

            var share = await _assignments.HaveSharedPersonAsync(
                professionalUserId, familyUserId, cancellationToken);

            if (!share)
                return ApiResponse<MessageResponse>.Forbidden(
                    "No tienes una relación activa con este usuario para enviarle mensajes.");

            // 7. Crear el mensaje
            var message = new Message
            {
                SenderId        = command.SenderId,
                ReceiverId      = command.ReceiverId,
                Subject         = command.Subject,
                Content         = command.Content,
                RelatedPersonId = command.RelatedPersonId,
                SentAt          = DateTime.UtcNow,
                IsRead          = false,
                IsActive        = true,
            };

            await _messages.CreateAsync(message, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Populate nav properties after save — setting them before EF Core's Add()
            // causes a PK_Users violation when the users were loaded with AsNoTracking.
            message.Sender   = sender;
            message.Receiver = receiver;

            return ApiResponse<MessageResponse>.SuccessResult(
                MessageMapper.ToDetail(message),
                "Mensaje enviado exitosamente.");
        }
    }
}
