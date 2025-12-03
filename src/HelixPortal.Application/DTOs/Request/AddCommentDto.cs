namespace HelixPortal.Application.DTOs.Request;

public class AddCommentDto
{
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false; // Only staff/admin can set this
}

