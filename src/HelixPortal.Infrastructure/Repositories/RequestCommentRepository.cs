using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Infrastructure.Repositories;

public class RequestCommentRepository : IRequestCommentRepository
{
    private readonly ApplicationDbContext _context;

    public RequestCommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RequestComment> CreateAsync(RequestComment comment, CancellationToken cancellationToken = default)
    {
        _context.RequestComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return comment;
    }

    public async Task<List<RequestComment>> GetByRequestIdAsync(
        Guid requestId,
        bool includeInternal = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RequestComments
            .Include(c => c.AuthorUser)
            .Where(c => c.RequestId == requestId)
            .AsQueryable();

        if (!includeInternal)
        {
            query = query.Where(c => !c.IsInternal);
        }

        return await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

