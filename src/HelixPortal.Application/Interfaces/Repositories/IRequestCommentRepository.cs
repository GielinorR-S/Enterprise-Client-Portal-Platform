using HelixPortal.Domain.Entities;

namespace HelixPortal.Application.Interfaces.Repositories;

public interface IRequestCommentRepository
{
    Task<RequestComment> CreateAsync(RequestComment comment, CancellationToken cancellationToken = default);
    Task<List<RequestComment>> GetByRequestIdAsync(Guid requestId, bool includeInternal = false, CancellationToken cancellationToken = default);
}

