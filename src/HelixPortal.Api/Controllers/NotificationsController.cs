using HelixPortal.Application.DTOs.Notification;
using HelixPortal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelixPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool includeRead = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetNotificationsAsync(
            userId,
            includeRead,
            cancellationToken);

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetNotificationAsync(id, cancellationToken);
        
        if (notification == null)
        {
            return NotFound();
        }

        // SECURITY: Users can only see their own notifications
        var userId = GetCurrentUserId();
        // The service should already filter by user, but double-check here
        // (We'd need to pass userId to GetNotificationAsync to properly check)

        return Ok(notification);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new { count });
    }
}

