using HelixPortal.Application.DTOs.Dashboard;
using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Services;

/// <summary>
/// Service for computing dashboard statistics.
/// </summary>
public class DashboardService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INotificationRepository _notificationRepository;

    public DashboardService(
        IRequestRepository requestRepository,
        IDocumentRepository documentRepository,
        INotificationRepository notificationRepository)
    {
        _requestRepository = requestRepository;
        _documentRepository = documentRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(
        Guid? clientOrganisationId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var openRequestsCount = await _requestRepository.GetOpenRequestsCountAsync(
            clientOrganisationId, 
            cancellationToken);

        var recentlyUpdatedCount = await _requestRepository.GetRecentlyUpdatedRequestsCountAsync(
            clientOrganisationId, 
            days: 7, 
            cancellationToken);

        var newDocumentsCount = await _documentRepository.GetNewDocumentsCountAsync(
            clientOrganisationId, 
            days: 7, 
            cancellationToken);

        var unreadNotificationsCount = userId.HasValue
            ? await _notificationRepository.GetUnreadCountAsync(userId.Value, cancellationToken)
            : 0;

        return new DashboardStatsDto
        {
            OpenRequestsCount = openRequestsCount,
            RecentlyUpdatedRequestsCount = recentlyUpdatedCount,
            NewDocumentsCount = newDocumentsCount,
            UnreadNotificationsCount = unreadNotificationsCount
        };
    }
}

