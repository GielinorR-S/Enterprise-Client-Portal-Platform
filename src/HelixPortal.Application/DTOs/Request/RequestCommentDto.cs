namespace HelixPortal.Application.DTOs.Request;

public class RequestCommentDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}

