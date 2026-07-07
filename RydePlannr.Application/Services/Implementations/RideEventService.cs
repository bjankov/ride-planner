using AutoMapper;
using RydePlannr.Application.DTOs.RideEvent;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Enums;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class RideEventService : IRideEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RideEventService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RideEventResponseDto> CreateAsync(
        CreateRideEventDto dto, int organizerId, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Repository<Route>().ExistsAsync(dto.RouteId, cancellationToken))
            throw new KeyNotFoundException($"Ruta s ID-em {dto.RouteId} nije pronađena.");

        if (!await _unitOfWork.Repository<RideType>().ExistsAsync(dto.RideTypeId, cancellationToken))
            throw new KeyNotFoundException($"Tip vožnje s ID-em {dto.RideTypeId} nije pronađen.");

        if (dto.ClubId is not null && !await _unitOfWork.Repository<Club>().ExistsAsync(dto.ClubId.Value, cancellationToken))
            throw new KeyNotFoundException($"Klub s ID-em {dto.ClubId} nije pronađen.");

        var rideEvent = _mapper.Map<RideEvent>(dto);
        rideEvent.OrganizerId = organizerId;
        rideEvent.Status = RideStatus.Planned;

        await _unitOfWork.Repository<RideEvent>().AddAsync(rideEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdRideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(rideEvent.Id, cancellationToken);
        return _mapper.Map<RideEventResponseDto>(createdRideEvent);
    }

    public async Task<RideEventResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(id, cancellationToken);
        if (rideEvent is null) return null;

        var dto = _mapper.Map<RideEventResponseDto>(rideEvent);
        var now = DateTime.UtcNow;

        if (rideEvent.Status == RideStatus.Cancelled)
            dto.Status = "Cancelled";
        else if (now < rideEvent.StartTime)
            dto.Status = "Planned";
        else if (rideEvent.EndTime.HasValue && now > rideEvent.EndTime)
            dto.Status = "Completed";
        else
            dto.Status = "Active";

        return dto;
    } 

    public async Task<IReadOnlyList<RideEventResponseDto>> GetAllUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var rides = await _unitOfWork.Rides.GetUpcomingRidesAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<RideEventResponseDto>>(rides);
    }
    
    public async Task<IReadOnlyList<RideEventResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rides = await _unitOfWork.Rides.GetAllWithDetailsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<RideEventResponseDto>>(rides);
    }
    
    public async Task<IReadOnlyList<RideEventResponseDto>> GetByClubAsync(
        int clubId, CancellationToken cancellationToken = default)
    {
        var rides = await _unitOfWork.Rides.GetRidesByClubAsync(clubId, cancellationToken);
        return _mapper.Map<IReadOnlyList<RideEventResponseDto>>(rides);
    }

    public async Task<RideEventResponseDto> UpdateAsync(
        int id, UpdateRideEventDto dto, int actingUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Repository<RideEvent>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Događaj s ID-em {id} nije pronađen.");

        // CHANGED: previously any authenticated user could edit any ride event.
        // Only the organizer who created it, or an Admin, may modify it now.
        if (!isAdmin && rideEvent.OrganizerId != actingUserId)
            throw new ForbiddenException("Nemate dozvolu za uređivanje ovog događaja.");

        if (dto.Title is not null) rideEvent.Title = dto.Title;
        if (dto.Description is not null) rideEvent.Description = dto.Description;
        if (dto.StartTime is not null) rideEvent.StartTime = dto.StartTime.Value;
        if (dto.EndTime is not null) rideEvent.EndTime = dto.EndTime.Value;
        if (dto.CutoffMinutes is not null) rideEvent.CutoffMinutes = dto.CutoffMinutes.Value;
        if (dto.MaxParticipants is not null) rideEvent.MaxParticipants = dto.MaxParticipants.Value;
        if (dto.Status is not null) rideEvent.Status = dto.Status.Value;

        await _unitOfWork.Repository<RideEvent>().UpdateAsync(rideEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(id, cancellationToken);
        return _mapper.Map<RideEventResponseDto>(updatedRideEvent);
    }

    public async Task DeleteAsync(int id, int actingUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Repository<RideEvent>().GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Događaj s ID-em {id} nije pronađen.");

        // CHANGED: previously only Admins could delete (via [Authorize(Roles = "Admin")]
        // at the controller), meaning an organizer couldn't delete their own event.
        // Same ownership rule as UpdateAsync: organizer or Admin.
        if (!isAdmin && rideEvent.OrganizerId != actingUserId)
            throw new ForbiddenException("Nemate dozvolu za brisanje ovog događaja.");

        await _unitOfWork.Repository<RideEvent>().DeleteAsync(rideEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task JoinRideAsync(int rideEventId, int userId, CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(rideEventId, cancellationToken)
            ?? throw new KeyNotFoundException($"Događaj s ID-em {rideEventId} nije pronađen.");

        if (rideEvent.Participants.Any(p => p.UserId == userId))
            throw new InvalidOperationException("Već ste prijavljeni na ovaj događaj.");

        if (rideEvent.Participants.Count >= rideEvent.MaxParticipants)
            throw new InvalidOperationException("Događaj je popunjen.");

        if (rideEvent.Status != RideStatus.Planned)
            throw new InvalidOperationException("Na ovaj događaj nije se moguće prijaviti.");

        var participant = new RideParticipant
        {
            RideEventId = rideEventId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<RideParticipant>().AddAsync(participant, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateFieldException)
        {
            throw new InvalidOperationException("Već ste prijavljeni na ovaj događaj.");
        }
    }

    public async Task LeaveRideAsync(int rideEventId, int userId, CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(rideEventId, cancellationToken)
            ?? throw new KeyNotFoundException($"Događaj s ID-em {rideEventId} nije pronađen.");

        var participant = rideEvent.Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Niste prijavljeni na ovaj događaj.");

        await _unitOfWork.Repository<RideParticipant>().DeleteAsync(participant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}