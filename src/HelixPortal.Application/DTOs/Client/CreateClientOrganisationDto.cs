namespace HelixPortal.Application.DTOs.Client;

public class CreateClientOrganisationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Timezone { get; set; }
}

