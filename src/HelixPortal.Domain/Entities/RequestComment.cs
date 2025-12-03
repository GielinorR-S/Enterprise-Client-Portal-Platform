namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a comment or message on a request. Can be visible to client or internal-only.
/// </summary>
public class RequestComment
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Request Request { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;
    
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; } // If true, only staff/admin can see this
    
    public DateTime CreatedAt { get; set; }
}

