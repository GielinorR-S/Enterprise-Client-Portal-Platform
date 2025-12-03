namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a client organisation that can have multiple users and requests.
/// </summary>
public class ClientOrganisation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? PrimaryContactId { get; set; }
    public User? PrimaryContact { get; set; }
    public string? Address { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

