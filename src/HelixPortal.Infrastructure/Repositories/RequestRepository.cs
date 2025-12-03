using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;
using HelixPortal.Domain.Enums;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly ApplicationDbContext _context;

    public RequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Request?> GetByIdAsync(Guid id, bool includeComments = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Requests
            .Include(r => r.ClientOrganisation)
            .Include(r => r.CreatedByUser)
            .AsQueryable();

        if (includeComments)
        {
            query = query.Include(r => r.Comments)
                .ThenInclude(c => c.AuthorUser);
        }

        return await query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Request>> GetByClientOrganisationIdAsync(
        Guid clientOrganisationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.ClientOrganisation)
            .Include(r => r.CreatedByUser)
            .Include(r => r.Comments)
            .Where(r => r.ClientOrganisationId == clientOrganisationId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Request>> GetByFiltersAsync(
        Guid? clientOrganisationId = null,
        RequestStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Requests
            .Include(r => r.ClientOrganisation)
            .Include(r => r.CreatedByUser)
            .AsQueryable();

        if (clientOrganisationId.HasValue)
        {
            query = query.Where(r => r.ClientOrganisationId == clientOrganisationId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Request> CreateAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<Request> UpdateAsync(Request request, CancellationToken cancellationToken = default)
    {
        request.UpdatedAt = DateTime.UtcNow;
        _context.Requests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<int> GetOpenRequestsCountAsync(
        Guid? clientOrganisationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Requests
            .Where(r => r.Status != RequestStatus.Closed && r.Status != RequestStatus.Resolved);

        if (clientOrganisationId.HasValue)
        {
            query = query.Where(r => r.ClientOrganisationId == clientOrganisationId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetRecentlyUpdatedRequestsCountAsync(
        Guid? clientOrganisationId = null,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        var query = _context.Requests
            .Where(r => r.UpdatedAt >= cutoffDate);

        if (clientOrganisationId.HasValue)
        {
            query = query.Where(r => r.ClientOrganisationId == clientOrganisationId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}

