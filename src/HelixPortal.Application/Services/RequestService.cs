using HelixPortal.Application.DTOs.Request;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Interfaces.Services;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for handling request (support ticket) operations.
/// </summary>
public class RequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IRequestCommentRepository _commentRepository;
    private readonly IClientOrganisationRepository _clientOrganisationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IServiceBusService _serviceBusService;
    private readonly IUserRepository _userRepository;

    public RequestService(
        IRequestRepository requestRepository,
        IRequestCommentRepository commentRepository,
        IClientOrganisationRepository clientOrganisationRepository,
        INotificationRepository notificationRepository,
        IServiceBusService serviceBusService,
        IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _commentRepository = commentRepository;
        _clientOrganisationRepository = clientOrganisationRepository;
        _notificationRepository = notificationRepository;
        _serviceBusService = serviceBusService;
        _userRepository = userRepository;
    }

    public async Task<RequestDto> CreateRequestAsync(
        CreateRequestDto dto, 
        Guid createdByUserId, 
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        var request = new Request
        {
            Id = Guid.NewGuid(),
            ClientOrganisationId = clientOrganisationId,
            CreatedByUserId = createdByUserId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = Enum.Parse<RequestPriority>(dto.Priority),
            Status = RequestStatus.New,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _requestRepository.CreateAsync(request, cancellationToken);

        // Publish event to Service Bus
        await _serviceBusService.PublishRequestCreatedEventAsync(
            created.Id, 
            clientOrganisationId, 
            cancellationToken);

        return MapToDto(created);
    }

    public async Task<RequestDetailDto?> GetRequestDetailAsync(
        Guid requestId, 
        Guid? currentUserId, 
        UserRole currentUserRole,
        Guid? currentUserOrganisationId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, includeComments: true, cancellationToken);
        
        if (request == null)
        {
            return null;
        }

        // SECURITY: Client users can only see requests from their own organisation
        if (currentUserRole == UserRole.Client && request.ClientOrganisationId != currentUserOrganisationId)
        {
            return null;
        }

        var dto = new RequestDetailDto
        {
            Id = request.Id,
            ClientOrganisationId = request.ClientOrganisationId,
            ClientOrganisationName = request.ClientOrganisation.Name,
            CreatedByUserId = request.CreatedByUserId,
            CreatedByUserName = request.CreatedByUser.DisplayName,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            StatusDisplay = request.Status.ToString(),
            Priority = request.Priority,
            PriorityDisplay = request.Priority.ToString(),
            DueDate = request.DueDate,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            CommentCount = request.Comments.Count
        };

        // Filter comments based on user role - clients can't see internal comments
        var visibleComments = request.Comments
            .Where(c => !c.IsInternal || currentUserRole != UserRole.Client)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new RequestCommentDto
            {
                Id = c.Id,
                RequestId = c.RequestId,
                AuthorUserId = c.AuthorUserId,
                AuthorUserName = c.AuthorUser.DisplayName,
                Message = c.Message,
                IsInternal = c.IsInternal,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        dto.Comments = visibleComments;

        return dto;
    }

    public async Task<List<RequestDto>> GetRequestsAsync(
        Guid? clientOrganisationId,
        RequestStatus? status,
        Guid? currentUserId,
        UserRole currentUserRole,
        Guid? currentUserOrganisationId,
        CancellationToken cancellationToken = default)
    {
        // SECURITY: Client users can only see requests from their own organisation
        if (currentUserRole == UserRole.Client)
        {
            clientOrganisationId = currentUserOrganisationId;
        }

        var requests = await _requestRepository.GetByFiltersAsync(
            clientOrganisationId,
            status,
            cancellationToken: cancellationToken);

        return requests.Select(MapToDto).ToList();
    }

    public async Task<RequestCommentDto> AddCommentAsync(
        Guid requestId,
        AddCommentDto dto,
        Guid authorUserId,
        UserRole authorRole,
        Guid? authorOrganisationId,
        CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken: cancellationToken);
        
        if (request == null)
        {
            throw new InvalidOperationException("Request not found");
        }

        // SECURITY: Client users can only comment on their own organisation's requests
        if (authorRole == UserRole.Client && request.ClientOrganisationId != authorOrganisationId)
        {
            throw new UnauthorizedAccessException("You do not have permission to comment on this request");
        }

        // SECURITY: Only staff/admin can create internal comments
        if (dto.IsInternal && authorRole == UserRole.Client)
        {
            dto.IsInternal = false;
        }

        var comment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            AuthorUserId = authorUserId,
            Message = dto.Message,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _commentRepository.CreateAsync(comment, cancellationToken);

        // Update request's UpdatedAt timestamp
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request, cancellationToken);

        // Fetch the author user for display name
        var authorUser = await _userRepository.GetByIdAsync(authorUserId, cancellationToken);
        var authorUserName = authorUser?.DisplayName ?? "Unknown User";

        return new RequestCommentDto
        {
            Id = created.Id,
            RequestId = created.RequestId,
            AuthorUserId = created.AuthorUserId,
            AuthorUserName = authorUserName,
            Message = created.Message,
            IsInternal = created.IsInternal,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<RequestDto?> UpdateRequestStatusAsync(
        Guid requestId,
        string status,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        // SECURITY: Only staff/admin can change request status
        if (currentUserRole == UserRole.Client)
        {
            throw new UnauthorizedAccessException("Only staff can update request status");
        }

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken: cancellationToken);
        
        if (request == null)
        {
            return null;
        }

        if (!Enum.TryParse<RequestStatus>(status, out var statusEnum))
        {
            throw new ArgumentException("Invalid status value");
        }

        request.Status = statusEnum;
        request.UpdatedAt = DateTime.UtcNow;

        var updated = await _requestRepository.UpdateAsync(request, cancellationToken);
        return MapToDto(updated);
    }

    private RequestDto MapToDto(Request request)
    {
        return new RequestDto
        {
            Id = request.Id,
            ClientOrganisationId = request.ClientOrganisationId,
            ClientOrganisationName = request.ClientOrganisation?.Name ?? string.Empty,
            CreatedByUserId = request.CreatedByUserId,
            CreatedByUserName = request.CreatedByUser?.DisplayName ?? string.Empty,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            StatusDisplay = request.Status.ToString(),
            Priority = request.Priority,
            PriorityDisplay = request.Priority.ToString(),
            DueDate = request.DueDate,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            CommentCount = request.Comments?.Count ?? 0
        };
    }
}

