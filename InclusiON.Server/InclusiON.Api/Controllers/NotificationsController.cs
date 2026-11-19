using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Notifications;

namespace InclusiON.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsRepository _repository;
    private readonly IHttpContextService _httpContextService;
    private readonly IDateTimeProvider _dateTime;

    public NotificationsController(
        INotificationsRepository repository,
        IHttpContextService httpContextService,
        IDateTimeProvider dateTime)
    {
        _repository         = repository;
        _httpContextService = httpContextService;
        _dateTime           = dateTime;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationResponse>>>> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] bool onlyUnread = false,
        CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var items = await _repository.GetByUserIdAsync(userId.Value, page, pageSize, onlyUnread, cancellationToken);
        var dtos = items.Select(NotificationResponse.From).ToList();
        return Ok(ApiResponse<List<NotificationResponse>>.SuccessResult(dtos));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<UnreadNotificationsCountResponse>>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var count = await _repository.GetUnreadCountAsync(userId.Value, cancellationToken);
        return Ok(ApiResponse<UnreadNotificationsCountResponse>.SuccessResult(new UnreadNotificationsCountResponse { Count = count }));
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var updated = await _repository.MarkAsReadAsync(id, userId.Value, _dateTime.UtcNow, cancellationToken);
        if (!updated)
            return NotFound(ApiResponse<object>.NotFound("Notificación"));

        return Ok(ApiResponse<object>.SuccessResult("Notificación marcada como leída."));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var count = await _repository.MarkAllAsReadAsync(userId.Value, _dateTime.UtcNow, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { updatedCount = count }));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Dismiss(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var dismissed = await _repository.DismissAsync(id, userId.Value, cancellationToken);
        if (!dismissed)
            return NotFound(ApiResponse<object>.NotFound("Notificación"));

        return Ok(ApiResponse<object>.SuccessResult("Notificación descartada."));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> DismissAll(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextService.GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var count = await _repository.DismissAllAsync(userId.Value, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResult(new { dismissedCount = count }));
    }
}
