using HelixPortal.Domain.Entities;

namespace HelixPortal.Application.Interfaces.Repositories;

public interface IClientOrganisationRepository
{
    Task<ClientOrganisation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ClientOrganisation>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ClientOrganisation> CreateAsync(ClientOrganisation organisation, CancellationToken cancellationToken = default);
    Task<ClientOrganisation> UpdateAsync(ClientOrganisation organisation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

