using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

[ApiController]
[Route("api/events")]

public class EventController : ControllerBase
{
    private readonly IEventService _eventService; 
    
    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost]
    public async Task<IActionResult>CreateEvent([FromBody] CreateEventRequest request)
    {
        var createdEvent= await _eventService.CreateEventAsync(request, 1); // hardcoded organizerId for now
        return Created(string.Empty, createdEvent); //201 created
        
    }
    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] PagedRequestDto request)
    {
        var response = await _eventService.GetAllEventsAsync(request);
        return Ok(response); //200 ok
    }
}
