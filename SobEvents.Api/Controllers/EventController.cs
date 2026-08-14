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


//create event

    [HttpPost]
    public async Task<IActionResult>CreateEvent([FromBody] CreateEventRequest request)
    {
        var createdEvent= await _eventService.CreateEventAsync(request, 1); // hardcoded organizerId for now
        return Created(string.Empty, createdEvent); //201 created
        
    }

//get all events
    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] PagedRequestDto request)
    {
        var response = await _eventService.GetAllEventsAsync(request);
        return Ok(response); //200 ok
    }
//get event by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var evt= await _eventService.GetEventByIdAsync(id);
        if(evt==null)
        {
            return NotFound(new { Message = $"Event with id {id} not found" });
        }
        return Ok(evt);
    }

//update event

[HttpPut("{id}")] 
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventRequest request)
    {
        // mocking organizerId = 1 
        var updatedEvent = await _eventService.UpdateEventAsync(id, request, 1);
        
        if (updatedEvent == null) 
        {
            return NotFound(new { message = "Event not found or you do not have permission to edit it." });
        }

        return Ok(updatedEvent);
    }

// delete event

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var success = await _eventService.DeleteEventAsync(id, 1);
        
        if (!success) 
        {
            return NotFound(new { message = "Event not found or you do not have permission to delete it." });
        }

        
        return NoContent(); 
    }
}
