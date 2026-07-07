using RydePlannr.Domain.Entities;

namespace RydePlannr.Domain.Interfaces;

public interface IRideRepository : IGenericRepository<RideEvent>
{
    Task<IReadOnlyList<RideEvent>> GetUpcomingRidesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEvent>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEvent>> GetRidesByClubAsync(int clubId, CancellationToken cancellationToken = default);
    Task<RideEvent?> GetRideWithParticipantsAsync(int rideEventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEvent>> GetRidesByUserAsync(int userId, CancellationToken cancellationToken = default);
}