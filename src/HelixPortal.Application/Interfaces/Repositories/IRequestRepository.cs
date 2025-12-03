using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Interfaces.Repositories;

public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(Guid id, bool includeComments = false, CancellationToken cancellationToken = default);
    Task<List<Request>> GetByClientOrganisationIdAsync(Guid clientOrganisationId, CancellationToken cancellationToken = default);
    Task<List<Request>> GetByFiltersAsync(
        Guid? clientOrganisationId = null,
        RequestStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
    Task<Request> CreateAsync(Request request, CancellationToken cancellationToken = default);
    Task<Request> UpdateAsync(Request request, CancellationToken cancellationToken = default);
    Task<int> GetOpenRequestsCountAsync(Guid? clientOrganisationId = null, CancellationToken cancellationToken = default);
    Task<int> GetRecentlyUpdatedRequestsCountAsync(Guid? clientOrganisationId = null, int days = 7, CancellationToken cancellationToken = default);
}

