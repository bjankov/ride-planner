using RydePlannr.Application.DTOs.User;

namespace RydePlannr.Application.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PublicUserResponseDto?> GetPublicByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}