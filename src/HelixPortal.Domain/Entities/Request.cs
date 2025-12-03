using HelixPortal.Domain.Enums;

namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a support request or ticket created by a client user.
/// </summary>
public class Request
{
    public Guid Id { get; set; }
    public Guid ClientOrganisationId { get; set; }
    public ClientOrganisation ClientOrganisation { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public RequestPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Navigation properties
    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

