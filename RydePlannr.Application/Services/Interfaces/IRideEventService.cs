using RydePlannr.Application.DTOs.RideEvent;

namespace RydePlannr.Application.Services.Interfaces;

public interface IRideEventService
{
    Task<RideEventResponseDto> CreateAsync(CreateRideEventDto dto, int organizerId, CancellationToken cancellationToken = default);
    Task<RideEventResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEventResponseDto>> GetAllUpcomingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEventResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RideEventResponseDto>> GetByClubAsync(int clubId, CancellationToken cancellationToken = default);
    Task<RideEventResponseDto> UpdateAsync(
        int id, UpdateRideEventDto dto, int actingUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int actingUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task JoinRideAsync(int rideEventId, int userId, CancellationToken cancellationToken = default);
    Task LeaveRideAsync(int rideEventId, int userId, CancellationToken cancellationToken = default);
}