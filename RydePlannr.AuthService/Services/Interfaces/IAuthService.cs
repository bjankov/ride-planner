using RydePlannr.AuthService.DTOs;

namespace RydePlannr.AuthService.Services.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<string> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
