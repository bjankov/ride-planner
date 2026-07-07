using AutoMapper;
using RydePlannr.Application.DTOs.Message;
using RydePlannr.Application.Services.Interfaces;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Exceptions;
using RydePlannr.Domain.Interfaces;

namespace RydePlannr.Application.Services.Implementations;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MessageResponseDto>> GetAllByRideEventAsync(
        int rideEventId, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Repository<RideEvent>().ExistsAsync(rideEventId, cancellationToken))
            throw new KeyNotFoundException($"Događaj s ID-em {rideEventId} nije pronađen.");

        var messages = await _unitOfWork.Messages.GetByRideEventWithUserAsync(rideEventId, cancellationToken);
        return _mapper.Map<IReadOnlyList<MessageResponseDto>>(messages);
    }

    public async Task<MessageResponseDto> CreateAsync(
        int rideEventId, int userId, CreateMessageDto messageDto,
        CancellationToken cancellationToken = default)
    {
        var rideEvent = await _unitOfWork.Rides.GetRideWithParticipantsAsync(rideEventId, cancellationToken)
            ?? throw new KeyNotFoundException($"Događaj s ID-em {rideEventId} nije pronađen.");

        if (rideEvent.Participants.All(p => p.UserId != userId))
            throw new ForbiddenException("Samo sudionici događaja mogu slati poruke.");

        var message = new RideMessage()
        {
            Content = messageDto.Content,
            RideEventId = rideEventId,
            UserId = userId,
            SentAt = DateTime.UtcNow,
            User = rideEvent.Participants.First(p => p.UserId == userId).User
        };

        await _unitOfWork.Repository<RideMessage>().AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MessageResponseDto>(message);
    }
}