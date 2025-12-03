namespace HelixPortal.Domain.Entities;

/// <summary>
/// Represents a document stored in Azure Blob Storage for a client organisation.
/// </summary>
public class Document
{
    public Guid Id { get; set; }
    public Guid ClientOrganisationId { get; set; }
    public ClientOrganisation ClientOrganisation { get; set; } = null!;
    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
    
    public string FileName { get; set; } = string.Empty;
    public string BlobStoragePath { get; set; } = string.Empty; // Full path in Azure Blob Storage
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DocumentCategory Category { get; set; }
    public int VersionNumber { get; set; }
    
    public DateTime UploadedAt { get; set; }
}

