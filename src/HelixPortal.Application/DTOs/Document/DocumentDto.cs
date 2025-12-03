using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.DTOs.Document;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid ClientOrganisationId { get; set; }
    public string ClientOrganisationName { get; set; } = string.Empty;
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeDisplay { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public string CategoryDisplay { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public DateTime UploadedAt { get; set; }
}

