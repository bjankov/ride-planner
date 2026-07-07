using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class RideMessageRepository : GenericRepository<RideMessage>, IRideMessageRepository
{
    public RideMessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<RideMessage>> GetByRideEventWithUserAsync(
        int rideEventId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.User)
            .Where(m => m.RideEventId == rideEventId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }
}
