using RydePlannr.Application.DTOs.Club;

namespace RydePlannr.Application.Services.Interfaces;

public interface IClubService
{
    Task<IReadOnlyList<ClubResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClubResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClubResponseDto> CreateAsync(CreateClubDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClubMemberResponseDto>> GetMembersAsync(int clubId, CancellationToken cancellationToken = default);
    Task JoinAsync(int clubId, int userId, CancellationToken cancellationToken = default);
    Task LeaveAsync(int clubId, int userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}