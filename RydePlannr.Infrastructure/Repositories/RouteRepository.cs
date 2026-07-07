using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class RouteRepository : GenericRepository<Route>, IRouteRepository
{
    public RouteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Route?> GetByIdWithLocationsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.StartLocation)
            .Include(r => r.EndLocation)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Route>> GetAllWithLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.StartLocation)
            .Include(r => r.EndLocation)
            .ToListAsync(cancellationToken);
    }
}
