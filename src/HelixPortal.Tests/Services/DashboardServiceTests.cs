using FluentAssertions;
using HelixPortal.Application.DTOs.Dashboard;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Application.Services;
using Moq;
using Xunit;

namespace HelixPortal.Tests.Services;

/// <summary>
/// Unit tests for DashboardService focusing on computing dashboard statistics.
/// Tests follow AAA pattern: Arrange, Act, Assert.
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();

        _dashboardService = new DashboardService(
            _requestRepositoryMock.Object,
            _documentRepositoryMock.Object,
            _notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldComputeAllStats_WhenValidInput()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _requestRepositoryMock
            .Setup(r => r.GetOpenRequestsCountAsync(organisationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _requestRepositoryMock
            .Setup(r => r.GetRecentlyUpdatedRequestsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _documentRepositoryMock
            .Setup(d => d.GetNewDocumentsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _notificationRepositoryMock
            .Setup(n => n.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        // Act
        var result = await _dashboardService.GetDashboardStatsAsync(
            organisationId,
            userId);

        // Assert
        result.Should().NotBeNull();
        result.OpenRequestsCount.Should().Be(5);
        result.RecentlyUpdatedRequestsCount.Should().Be(3);
        result.NewDocumentsCount.Should().Be(2);
        result.UnreadNotificationsCount.Should().Be(4);

        _requestRepositoryMock.Verify(r => r.GetOpenRequestsCountAsync(organisationId, It.IsAny<CancellationToken>()), Times.Once);
        _requestRepositoryMock.Verify(r => r.GetRecentlyUpdatedRequestsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()), Times.Once);
        _documentRepositoryMock.Verify(d => d.GetNewDocumentsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()), Times.Once);
        _notificationRepositoryMock.Verify(n => n.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnZeroCounts_WhenNoData()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _requestRepositoryMock
            .Setup(r => r.GetOpenRequestsCountAsync(organisationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _requestRepositoryMock
            .Setup(r => r.GetRecentlyUpdatedRequestsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _documentRepositoryMock
            .Setup(d => d.GetNewDocumentsCountAsync(organisationId, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _notificationRepositoryMock
            .Setup(n => n.GetUnreadCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _dashboardService.GetDashboardStatsAsync(
            organisationId,
            userId);

        // Assert
        result.Should().NotBeNull();
        result.OpenRequestsCount.Should().Be(0);
        result.RecentlyUpdatedRequestsCount.Should().Be(0);
        result.NewDocumentsCount.Should().Be(0);
        result.UnreadNotificationsCount.Should().Be(0);
    }
}

