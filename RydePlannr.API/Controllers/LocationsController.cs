using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.DTOs.Location;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var locations = await _locationService.GetAllAsync(cancellationToken);
        return Ok(locations);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var location = await _locationService.GetByIdAsync(id, cancellationToken);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var location = await _locationService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _locationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}