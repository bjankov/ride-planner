using RydePlannr.Application.DTOs.Message;

namespace RydePlannr.Application.Services.Interfaces;

public interface IMessageService
{
    Task<IReadOnlyList<MessageResponseDto>> GetAllByRideEventAsync(int rideEventId, CancellationToken cancellationToken =  default);
    Task<MessageResponseDto> CreateAsync (int rideEventId, int userId, CreateMessageDto messageDto, CancellationToken cancellationToken = default);
}