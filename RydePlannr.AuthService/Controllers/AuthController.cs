using Microsoft.AspNetCore.Mvc;
using RydePlannr.AuthService.DTOs;
using RydePlannr.AuthService.Services.Interfaces;

namespace RydePlannr.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshRequestDto dto, CancellationToken cancellationToken)
    {
        await _authService.RevokeAsync(dto, cancellationToken);
        return NoContent();
    }
}
