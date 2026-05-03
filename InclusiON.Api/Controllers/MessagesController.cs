using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Api.Extensions;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.Messages.Commands;
using InclusiON.Application.UseCases.Messages.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Messages;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Messages;

namespace InclusiON.Api.Controllers
{
    /// <summary>
    /// Controlador para el sistema de mensajeria entre usuarios.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly IHttpContextService _httpContextService;

        public MessagesController(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        // ────────────────────────────────────────────────────────────────
        // Queries
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene la bandeja de entrada paginada del usuario autenticado.
        /// Solo mensajes de nivel superior (no respuestas).
        /// </summary>
        [HttpGet("inbox")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<MessageListItemResponse>>>> GetInbox(
            [FromServices] IQueryHandler<GetInboxQuery, ApiResponse<PagedResponse<MessageListItemResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] Guid? relatedPersonId = null,
            [FromQuery] Guid? senderId = null,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await handler.HandleAsync(
                new GetInboxQuery(userId.Value, page, pageSize, isRead, relatedPersonId, senderId),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los mensajes enviados paginados del usuario autenticado.
        /// </summary>
        [HttpGet("sent")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<MessageListItemResponse>>>> GetSent(
            [FromServices] IQueryHandler<GetSentQuery, ApiResponse<PagedResponse<MessageListItemResponse>>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] Guid? relatedPersonId = null,
            [FromQuery] Guid? receiverId = null,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await handler.HandleAsync(
                new GetSentQuery(userId.Value, page, pageSize, isRead, relatedPersonId, receiverId),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle de un mensaje. Se marca como leído automáticamente si es el destinatario.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> GetMessage(
            int id,
            [FromServices] IQueryHandler<GetMessageByIdQuery, ApiResponse<MessageResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new GetMessageByIdQuery(id, userId.Value), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Devuelve los contactos disponibles para mensajería del usuario autenticado.
        /// Profesional → familiares de sus personas activas.
        /// Familiar → profesionales de sus personas activas.
        /// </summary>
        [HttpGet("contacts")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<List<MessageContactResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<MessageContactResponse>>>> GetContacts(
            [FromServices] IQueryHandler<GetMessageContactsQuery, ApiResponse<List<MessageContactResponse>>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new GetMessageContactsQuery(userId.Value), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Devuelve la cantidad de mensajes no leídos del usuario autenticado.
        /// </summary>
        [HttpGet("unread-count")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UnreadCountResponse>>> GetUnreadCount(
            [FromServices] IQueryHandler<GetUnreadCountQuery, ApiResponse<UnreadCountResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new GetUnreadCountQuery(userId.Value), cancellationToken);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────────────────
        // Commands
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Envía un nuevo mensaje a otro usuario.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Messages.Create)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> SendMessage(
            [FromBody] SendMessageRequest request,
            [FromServices] ICommandHandler<SendMessageCommand, ApiResponse<MessageResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var command = new SendMessageCommand(
                userId.Value,
                request.ReceiverId,
                request.Subject,
                request.Content,
                request.RelatedPersonId);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetMessage), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Responde a un mensaje existente. Solo participantes del hilo pueden responder.
        /// </summary>
        [HttpPost("{id:int}/reply")]
        [Authorize(Policy = Permissions.Messages.Create)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> ReplyToMessage(
            int id,
            [FromBody] ReplyToMessageRequest request,
            [FromServices] ICommandHandler<ReplyToMessageCommand, ApiResponse<MessageResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(
                new ReplyToMessageCommand(userId.Value, id, request.Content), cancellationToken);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(GetMessage), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Marca un mensaje como leído manualmente. Solo el destinatario puede hacerlo.
        /// </summary>
        [HttpPut("{id:int}/read")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponse>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<MessageResponse>>> MarkAsRead(
            int id,
            [FromServices] ICommandHandler<MarkMessageReadCommand, ApiResponse<MessageResponse>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new MarkMessageReadCommand(id, userId.Value), cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Elimina un mensaje (soft delete). Remitente o destinatario pueden eliminarlo.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.Messages.Read)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMessage(
            int id,
            [FromServices] ICommandHandler<DeleteMessageCommand, ApiResponse<object>> handler,
            CancellationToken cancellationToken = default)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await handler.HandleAsync(new DeleteMessageCommand(id, userId.Value), cancellationToken);
            return result.ToActionResult();
        }
    }
}
