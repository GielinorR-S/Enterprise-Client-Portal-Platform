using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.DTOs.Request;

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid ClientOrganisationId { get; set; }
    public string ClientOrganisationName { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public RequestPriority Priority { get; set; }
    public string PriorityDisplay { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
}

