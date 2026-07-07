using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.DTOs.Route;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RoutesController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var routes = await _routeService.GetAllAsync(cancellationToken);
        return Ok(routes);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var route = await _routeService.GetByIdAsync(id, cancellationToken);
        return route is null ? NotFound() : Ok(route);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRouteDto dto, CancellationToken cancellationToken)
    {
        var route = await _routeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _routeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}