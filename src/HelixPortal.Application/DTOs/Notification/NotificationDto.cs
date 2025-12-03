using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string TypeDisplay { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public Guid? RelatedRequestId { get; set; }
    public Guid? RelatedDocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

