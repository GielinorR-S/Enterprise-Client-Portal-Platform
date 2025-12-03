using HelixPortal.Application.DTOs.Notification;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for handling notification operations.
/// </summary>
public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        bool includeRead = false,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(
            userId,
            includeRead,
            cancellationToken);

        return notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<NotificationDto?> GetNotificationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        
        if (notification == null)
        {
            return null;
        }

        return MapToDto(notification);
    }

    public async Task MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAsReadAsync(id, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string message,
        Guid? relatedRequestId = null,
        Guid? relatedDocumentId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = false,
            RelatedRequestId = relatedRequestId,
            RelatedDocumentId = relatedDocumentId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _notificationRepository.CreateAsync(notification, cancellationToken);
        return MapToDto(created);
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            TypeDisplay = notification.Type.ToString(),
            Message = notification.Message,
            IsRead = notification.IsRead,
            RelatedRequestId = notification.RelatedRequestId,
            RelatedDocumentId = notification.RelatedDocumentId,
            CreatedAt = notification.CreatedAt
        };
    }
}

