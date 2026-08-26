using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SobEvents.Application.Commands.Events;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Events;

namespace SobEvents.Api.Controllers;

/// <summary>
/// event creation, discovery
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events")]
[Produces("application/json")]
[EnableRateLimiting("general-limiter")]
public class EventsController(ISender mediator, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Creates a new draft event. Requires Organizer role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Organizer")] // Role-based Authorization
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var command = new CreateEventCommand(
            request.Name, request.Description, request.StartDate,
            request.EndDate, request.Location, request.ImageUrl, currentUser.UserId!.Value 
        );

        var createdEvent = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    /// <summary>
    /// Retrieves a paginated list of events with optional search filtering (Public).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<EventResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents([FromQuery] PagedRequestDto request, CancellationToken ct)
    {
        var response = await mediator.Send(new GetEventsPagedQuery(request), ct);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves full event details including HATEOAS action links (Public).
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

    /// <summary>
    /// Updates an existing event. Requires Organizer role and ownership.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var command = new UpdateEventCommand(
            id, request.Name, request.Description, request.StartDate,
            request.EndDate, request.Location, request.ImageUrl, currentUser.UserId!.Value
        );

        var updatedEvent = await mediator.Send(command, ct);
        if (updatedEvent == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or unauthorized." });
        return Ok(updatedEvent);
    }

    /// <summary>
    /// Soft-deletes an event from the catalog. Requires Organizer role.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id, CancellationToken ct)
    {
        var success = await mediator.Send(new DeleteEventCommand(id, currentUser.UserId!.Value), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or unauthorized." });
        return NoContent();
    }

    /// <summary>
    /// Publishes a draft event to make it visible to attendees. Requires Organizer role.
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PublishEvent(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new PublishEventCommand(id, currentUser.UserId!.Value), ct);

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

    /// <summary>
    /// Cancels an event. Requires Organizer role.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelEvent(int id, CancellationToken ct)
    {
        var success = await mediator.Send(new CancelEventCommand(id, currentUser.UserId!.Value), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or already cancelled." });
        return NoContent();
    }
}