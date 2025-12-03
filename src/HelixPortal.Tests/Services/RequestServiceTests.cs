using FluentAssertions;
using HelixPortal.Application.DTOs.Request;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Interfaces.Services;
using HelixPortal.Application.Services;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;
using Moq;
using Xunit;

namespace HelixPortal.Tests.Services;

/// <summary>
/// Unit tests for RequestService focusing on business logic with mocked repositories.
/// Tests follow AAA pattern: Arrange, Act, Assert.
/// </summary>
public class RequestServiceTests
{
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly Mock<IRequestCommentRepository> _commentRepositoryMock;
    private readonly Mock<IClientOrganisationRepository> _clientOrganisationRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IServiceBusService> _serviceBusServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RequestService _requestService;

    public RequestServiceTests()
    {
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _commentRepositoryMock = new Mock<IRequestCommentRepository>();
        _clientOrganisationRepositoryMock = new Mock<IClientOrganisationRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _serviceBusServiceMock = new Mock<IServiceBusService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _requestService = new RequestService(
            _requestRepositoryMock.Object,
            _commentRepositoryMock.Object,
            _clientOrganisationRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _serviceBusServiceMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequest_WhenValidInput()
    {
        // Arrange
        var dto = new CreateRequestDto
        {
            Title = "Test Request",
            Description = "This is a test request",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var clientOrganisation = new ClientOrganisation
        {
            Id = organisationId,
            Name = "Test Client"
        };

        var createdRequest = new Request
        {
            Id = Guid.NewGuid(),
            ClientOrganisationId = organisationId,
            CreatedByUserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = RequestPriority.High,
            Status = RequestStatus.New,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ClientOrganisation = clientOrganisation,
            CreatedByUser = new User { Id = userId, DisplayName = "Test User" }
        };

        _requestRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRequest);

        _serviceBusServiceMock
            .Setup(s => s.PublishRequestCreatedEventAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _requestService.CreateRequestAsync(dto, userId, organisationId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.Priority.Should().Be(RequestPriority.High);
        result.Status.Should().Be(RequestStatus.New);
        
        _requestRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once);
        _serviceBusServiceMock.Verify(s => s.PublishRequestCreatedEventAsync(
            It.Is<Guid>(id => id == createdRequest.Id),
            It.Is<Guid>(id => id == organisationId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddComment_WhenValidInput()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            ClientOrganisationId = organisationId,
            CreatedByUserId = authorId,
            Title = "Existing Request",
            Status = RequestStatus.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ClientOrganisation = new ClientOrganisation { Id = organisationId },
            CreatedByUser = new User { Id = authorId }
        };

        var commentDto = new AddCommentDto
        {
            Message = "This is a test comment",
            IsInternal = false
        };

        var createdComment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            AuthorUserId = authorId,
            Message = commentDto.Message,
            IsInternal = false,
            CreatedAt = DateTime.UtcNow,
            AuthorUser = new User { Id = authorId, DisplayName = "Test User" }
        };

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(requestId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        _commentRepositoryMock
            .Setup(c => c.CreateAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdComment);

        _requestRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = authorId, DisplayName = "Test User" });

        // Act
        var result = await _requestService.AddCommentAsync(
            requestId,
            commentDto,
            authorId,
            UserRole.Client,
            organisationId);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be(commentDto.Message);
        result.RequestId.Should().Be(requestId);
        
        _commentRepositoryMock.Verify(c => c.CreateAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()), Times.Once);
        _requestRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldPreventInternalComment_WhenUserIsClient()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var existingRequest = new Request
        {
            Id = requestId,
            ClientOrganisationId = organisationId,
            CreatedByUserId = authorId,
            Title = "Existing Request",
            Status = RequestStatus.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ClientOrganisation = new ClientOrganisation { Id = organisationId },
            CreatedByUser = new User { Id = authorId }
        };

        var commentDto = new AddCommentDto
        {
            Message = "This comment should not be internal",
            IsInternal = true // Client trying to make internal comment
        };

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(requestId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        // Act
        var result = await _requestService.AddCommentAsync(
            requestId,
            commentDto,
            authorId,
            UserRole.Client,
            organisationId);

        // Assert
        // The service should override IsInternal to false for client users
        // This is verified by checking the comment created doesn't have IsInternal=true
        _commentRepositoryMock.Verify(c => c.CreateAsync(
            It.Is<RequestComment>(cmt => cmt.IsInternal == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

