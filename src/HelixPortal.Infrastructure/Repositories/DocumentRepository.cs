using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.ClientOrganisation)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Document>> GetByClientOrganisationIdAsync(
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.ClientOrganisation)
            .Include(d => d.UploadedByUser)
            .Where(d => d.ClientOrganisationId == clientOrganisationId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document> CreateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task<int> GetNewDocumentsCountAsync(
        Guid? clientOrganisationId = null,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _context.Documents
            .Where(d => d.UploadedAt >= cutoffDate);

        if (clientOrganisationId.HasValue)
        {
            query = query.Where(d => d.ClientOrganisationId == clientOrganisationId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}

