using HelixPortal.Domain.Enums;

namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a notification for a user about updates to requests, documents, etc.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    
    // Optional links to related entities
    public Guid? RelatedRequestId { get; set; }
    public Guid? RelatedDocumentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

