using RydePlannr.Application.DTOs.Route;

namespace RydePlannr.Application.Services.Interfaces;

public interface IRouteService
{
    Task<IReadOnlyList<RouteResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RouteResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RouteResponseDto> CreateAsync(CreateRouteDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}