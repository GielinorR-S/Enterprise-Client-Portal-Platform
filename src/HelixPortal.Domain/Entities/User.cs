namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a user in the system. Can be either a staff/admin user or a client user.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    
    // For client users, this links to their organisation
    public Guid? ClientOrganisationId { get; set; }
    public ClientOrganisation? ClientOrganisation { get; set; }
    
    // Navigation properties
    public ICollection<Request> CreatedRequests { get; set; } = new List<Request>();
    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

