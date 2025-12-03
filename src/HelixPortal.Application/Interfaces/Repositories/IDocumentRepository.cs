using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;

namespace HelixPortal.Application.Interfaces.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByClientOrganisationIdAsync(Guid clientOrganisationId, CancellationToken cancellationToken = default);
    Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default);
    Task<int> GetNewDocumentsCountAsync(Guid? clientOrganisationId = null, int days = 7, CancellationToken cancellationToken = default);
}

