using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.DTOs.Club;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClubsController : ControllerBase
{
    private readonly IClubService _clubService;
    private readonly IRideEventService _rideEventService;

    public ClubsController(IClubService clubService,  IRideEventService rideEventService)
    {
        _clubService = clubService;
        _rideEventService = rideEventService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var clubs = await _clubService.GetAllAsync(cancellationToken);
        return Ok(clubs);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var club = await _clubService.GetByIdAsync(id, cancellationToken);
        return club is null ? NotFound() : Ok(club);
    }

    [HttpGet("{id}/members")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMembers(int id, CancellationToken cancellationToken)
    {
        var members = await _clubService.GetMembersAsync(id, cancellationToken);
        return Ok(members);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClubDto dto, CancellationToken cancellationToken)
    {
        var club = await _clubService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = club.Id }, club);
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _clubService.JoinAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _clubService.LeaveAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _clubService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
    
    [HttpGet("{id}/rideevents")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRideEvents(int id, CancellationToken cancellationToken)
    {
        var rides = await _rideEventService.GetByClubAsync(id, cancellationToken);
        return Ok(rides);
    }
}