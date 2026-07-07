using RydePlannr.Application.DTOs.Location;

namespace RydePlannr.Application.Services.Interfaces;

public interface ILocationService
{
    Task<IReadOnlyList<LocationResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LocationResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LocationResponseDto> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}