using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RydePlannr.Application.DTOs.Message;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.API.Controllers;

[ApiController]
[Route("api/rideevents/{rideEventId}/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    // TODO: Turn this into a primary constructor?
    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(int rideEventId, CancellationToken cancellationToken)
    {
        var messages = await _messageService.GetAllByRideEventAsync(rideEventId, cancellationToken);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int rideEventId, CreateMessageDto dto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var message = await _messageService.CreateAsync(rideEventId, userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { rideEventId }, message);
    }
}