using RydePlannr.AuthService.DTOs;

namespace RydePlannr.AuthService.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshRequestDto dto, CancellationToken cancellationToken = default);
}
