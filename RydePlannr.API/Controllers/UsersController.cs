using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        if (isAdmin || callerId == id)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            return user is null ? NotFound() : Ok(user);
        }

        var publicUser = await _userService.GetPublicByIdAsync(id, cancellationToken);
        return publicUser is null ? NotFound() : Ok(publicUser);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}