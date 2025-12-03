namespace HelixPortal.Application.DTOs.Client;

public class ClientOrganisationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? PrimaryContactId { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? Address { get; set; }
    public string? Timezone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

