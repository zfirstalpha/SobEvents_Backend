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

// publish event 
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishEvent(int id, CancellationToken ct)
    {
        var (success, errorMessage) = await _eventService.PublishEventAsync(id, 1, ct); // Mock Organizer Id 1

        if (!success)
        {
            if (errorMessage == "Event not found or unauthorized.")
            {
                return NotFound(new { message = errorMessage });
            }

            //  409 Conflict for business rule violation (e.g. no tickets)
            return Conflict(new ProblemDetails
            {
                Title = "Publishing Conflict",
                Detail = errorMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        return NoContent(); 
    }

// cancel event
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelEvent(int id, CancellationToken ct)
    {
        var success = await _eventService.CancelEventAsync(id, 1, ct); // Mock Organizer Id 1

        if (!success)
        {
            return NotFound(new { message = "Event not found, unauthorized, or already cancelled." });
        }

        return NoContent();
    }
}
