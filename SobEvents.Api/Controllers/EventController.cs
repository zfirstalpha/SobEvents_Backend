using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Commands;
using SobEvents.Application.Queries;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Manages event creation, discovery, lifecycle states, and updates.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
[Produces("application/json")]

public class EventsController(ISender mediator) : ControllerBase
{


//create event

 /// <summary>
    /// Creates a new draft event.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct)
    {
      //  map dto to command with mock organizer id 1
        var command = new CreateEventCommand(
            request.Name, request.Description, request.StartDate,
            request.EndDate, request.Location, request.ImageUrl, 1
        );
        
        //  MediatR runs FluentValidation behavior and dispatches to the Handler!
        var createdEvent = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetEvents), new { id = createdEvent.Id }, createdEvent);

    }

//get all events

/// <summary>
    /// Retrieves a paginated list of published events with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<EventResponseDto>), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetEvents([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var response = await mediator.Send(new GetEventsPagedQuery(request), ct);
        return Ok(response);
    }


//get event by id

    /// <summary>
    /// Retrieves full event details including HATEOAS action links.
    /// </summary>
    [HttpGet("{id}")]
     [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventById(int id, CancellationToken ct)
    {
        var evt = await mediator.Send(new GetEventByIdQuery(id), ct);
        if (evt == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Event with ID {id} not found." });
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
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var command = new UpdateEventCommand(
            id, request.Name, request.Description, request.StartDate,
            request.EndDate, request.Location, request.ImageUrl, 1 // Mock Organizer Id 1
        );

        var updatedEvent = await mediator.Send(command, ct);
        if (updatedEvent == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or unauthorized." });
        return Ok(updatedEvent);
    }

// delete event

    /// <summary>
    /// Soft-deletes an event from the catalog.
    /// </summary>

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id, CancellationToken ct)
    {
        var success = await mediator.Send(new DeleteEventCommand(id, 1), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or unauthorized." });
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
        var result = await mediator.Send(new PublishEventCommand(id, 1), ct);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Event not found or unauthorized.")
            {
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = result.ErrorMessage });
            }

            return Conflict(new ProblemDetails
            {
                Title = "Publishing Conflict",
                Detail = result.ErrorMessage,
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
        var success = await mediator.Send(new CancelEventCommand(id, 1), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or already cancelled." });
        return NoContent();
    }
}
