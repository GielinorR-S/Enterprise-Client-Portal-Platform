namespace HelixPortal.Application.DTOs.Document;

public class UploadDocumentDto
{
    public Guid ClientOrganisationId { get; set; }
    public string Category { get; set; } = "General";
}

