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
        var token = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto, CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(new { Token = token });
    }
}
