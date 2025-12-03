using HelixPortal.Application.DTOs.Request;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.DTOs.Request;

public class RequestDetailDto : RequestDto
{
    public List<RequestCommentDto> Comments { get; set; } = new();
}

