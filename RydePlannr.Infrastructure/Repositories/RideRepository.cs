using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Enums;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Repositories;

public class RideRepository : GenericRepository<RideEvent>, IRideRepository
{
    public RideRepository(ApplicationDbContext context) : base(context)
    {
    }

    // RideEventResponseDto needs Organizer/Route/RideType/Club/Participants for every listing —
    // without these includes, AutoMapper silently maps names to null and CurrentParticipants to 0.
    private IQueryable<RideEvent> WithDetails() => DbSet
        .Include(r => r.Organizer)
        .Include(r => r.Route)
        .Include(r => r.RideType)
        .Include(r => r.Club)
        .Include(r => r.Participants);

    public async Task<IReadOnlyList<RideEvent>> GetUpcomingRidesAsync(
        CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .Where(r => r.StartTime > DateTime.UtcNow && r.Status != RideStatus.Cancelled)
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RideEvent>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RideEvent>> GetRidesByClubAsync(
        int clubId,
        CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .Where(r => r.ClubId == clubId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<RideEvent?> GetRideWithParticipantsAsync(
        int rideEventId,
        CancellationToken cancellationToken = default)
    {
        // Re-stating .Include(Participants) here (already in WithDetails) is required to chain
        // .ThenInclude(User) — EF merges it with the existing include rather than duplicating the join.
        return await WithDetails()
            .Include(r => r.Participants)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == rideEventId, cancellationToken);
    }

    public async Task<IReadOnlyList<RideEvent>> GetRidesByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await WithDetails()
            .Where(r => r.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }
}