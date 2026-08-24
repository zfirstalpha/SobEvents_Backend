using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Manages event creation, discovery, lifecycle states, and updates.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
[Produces("application/json")]

public class EventController : ControllerBase
{
    private readonly IEventService _eventService; 
    
    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }


//create event

 /// <summary>
    /// Creates a new draft event.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult>CreateEvent([FromBody] CreateEventRequest request)
    {
        var createdEvent= await _eventService.CreateEventAsync(request, 1); // hardcoded organizerId for now
        return Created(string.Empty, createdEvent); //201 created
        
    }

//get all events

/// <summary>
    /// Retrieves a paginated list of published events with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<EventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents([FromQuery] PagedRequestDto request)
    {
        var response = await _eventService.GetAllEventsAsync(request);
        return Ok(response); //200 ok
    }


//get event by id

    /// <summary>
    /// Retrieves full event details including HATEOAS action links.
    /// </summary>
    [HttpGet("{id}")]
     [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Updates an existing event's details.
    /// </summary>

[HttpPut("{id}")] 
[ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Soft-deletes an event from the catalog.
    /// </summary>

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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

        /// <summary>
    /// Publishes a draft event to make it visible to attendees.
    /// </summary>
    
    [HttpPost("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

        /// <summary>
    /// Cancels an event and alerts registered attendees.
    /// </summary>
    
    [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
