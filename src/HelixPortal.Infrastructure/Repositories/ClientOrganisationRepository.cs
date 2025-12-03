using HelixPortal.Application.Interfaces.Repositories;
using HelixPortal.Domain.Entities;
using HelixPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Infrastructure.Repositories;

public class ClientOrganisationRepository : IClientOrganisationRepository
{
    private readonly ApplicationDbContext _context;

    public ClientOrganisationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientOrganisation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ClientOrganisations
            .Include(co => co.PrimaryContact)
            .FirstOrDefaultAsync(co => co.Id == id, cancellationToken);
    }

    public async Task<List<ClientOrganisation>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.ClientOrganisations
            .Include(co => co.PrimaryContact)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(co => co.IsActive);
        }

        return await query
            .OrderBy(co => co.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientOrganisation> CreateAsync(ClientOrganisation organisation, CancellationToken cancellationToken = default)
    {
        _context.ClientOrganisations.Add(organisation);
        await _context.SaveChangesAsync(cancellationToken);
        return organisation;
    }

    public async Task<ClientOrganisation> UpdateAsync(ClientOrganisation organisation, CancellationToken cancellationToken = default)
    {
        organisation.UpdatedAt = DateTime.UtcNow;
        _context.ClientOrganisations.Update(organisation);
        await _context.SaveChangesAsync(cancellationToken);
        return organisation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organisation = await _context.ClientOrganisations.FindAsync(new object[] { id }, cancellationToken);
        if (organisation != null)
        {
            organisation.IsActive = false;
            organisation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

