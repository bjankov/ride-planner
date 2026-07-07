using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.DTOs.RideEvent;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RideEventsController : ControllerBase
{
    private readonly IRideEventService _rideEventService;

    public RideEventsController(IRideEventService rideEventService)
    {
        _rideEventService = rideEventService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllUpcoming(CancellationToken cancellationToken)
    {
        var rides = await _rideEventService.GetAllUpcomingAsync(cancellationToken);
        return Ok(rides);
    }
    
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var rides = await _rideEventService.GetAllAsync(cancellationToken);
        return Ok(rides);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var ride = await _rideEventService.GetByIdAsync(id, cancellationToken);
        return ride is null ? NotFound() : Ok(ride);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRideEventDto dto, CancellationToken cancellationToken)
    {
        var organizerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ride = await _rideEventService.CreateAsync(dto, organizerId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ride.Id }, ride);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, UpdateRideEventDto dto, CancellationToken cancellationToken)
    {
        var actingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var ride = await _rideEventService.UpdateAsync(id, dto, actingUserId, isAdmin, cancellationToken);
        return Ok(ride);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var actingUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        await _rideEventService.DeleteAsync(id, actingUserId, isAdmin, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _rideEventService.JoinRideAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _rideEventService.LeaveRideAsync(id, userId, cancellationToken);
        return NoContent();
    }
}