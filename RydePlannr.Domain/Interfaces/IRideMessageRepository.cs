using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IRideMessageRepository : IGenericRepository<RideMessage>
{
    Task<IReadOnlyList<RideMessage>> GetByRideEventWithUserAsync(
        int rideEventId, CancellationToken cancellationToken = default);
}
