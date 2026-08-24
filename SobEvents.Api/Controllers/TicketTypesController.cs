using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Commands.TicketTypes;
using SobEvents.Application.Queries.TicketTypes;
using MediatR;
namespace SobEvents.Api.Controllers;

/// <summary>
/// Manages ticket tiers (VIP, General) and capacity limits for events.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/events/{eventId}/tickets")]//nested restufull routing
[Produces("application/json")]

public class TicketTypesController(ISender mediator) : ControllerBase
{
//create a new ticket type for an event

    /// <summary>
    /// Creates a new ticket tier for an event.
    /// </summary>
    
    [HttpPost]
    [ProducesResponseType(typeof(TicketTypeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
     public async Task<IActionResult> CreateTicketType(int eventId, [FromBody] CreateTicketTypeRequest request, CancellationToken ct)
    {
        var command = new CreateTicketTypeCommand(
            eventId, request.Name, request.Price, request.Quantity,
            request.StartDate, request.EndDate, 1 // Mock Organizer Id 1
        );

        var ticketType = await mediator.Send(command, ct);
        if (ticketType == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Event not found or unauthorized." });

        return CreatedAtAction(nameof(GetTicketTypeById), new { eventId, id = ticketType.Id }, ticketType);
    }

    
//get ticket types

        /// <summary>
    /// Retrieves all ticket tiers for an event with real-time remaining capacity.
    /// </summary>

[HttpGet]
[ProducesResponseType(typeof(List<TicketTypeResponseDto>), StatusCodes.Status200OK)]
     public async Task<IActionResult> GetTicketTypes(int eventId, CancellationToken ct)
    {
        var tickets = await mediator.Send(new GetTicketTypesByEventQuery(eventId), ct);
        return Ok(tickets);
    }
//get tickettype by id

/// <summary>
    /// Retrieves a single ticket tier by ID.
    /// </summary>
 [HttpGet("{id}")]
 [ProducesResponseType(typeof(TicketTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
   public async Task<IActionResult> GetTicketTypeById(int eventId, int id, CancellationToken ct)
    {
        var ticket = await mediator.Send(new GetTicketTypeByIdQuery(eventId, id), ct);
        if (ticket == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Ticket type not found." });
        return Ok(ticket);
    }
//update ticket 

 /// <summary>
    /// Updates pricing or capacity for a ticket tier.
    /// </summary>
     [HttpPut("{id}")]
      [ProducesResponseType(typeof(TicketTypeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicketType(int eventId, int id, [FromBody] UpdateTicketTypeRequest request, CancellationToken ct)
    {
        var command = new UpdateTicketTypeCommand(
            id, eventId, request.Name, request.Price, request.Quantity,
            request.StartDate, request.EndDate, request.IsActive, 1 // Mock Organizer Id 1
        );

        var updated = await mediator.Send(command, ct);
        if (updated == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Ticket type not found or unauthorized." });
        return Ok(updated);
    }
//delete ticket type 

/// <summary>
    /// Deletes or deactivates a ticket tier.
    /// </summary>
     [HttpDelete("{id}")]
     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
   public async Task<IActionResult> DeleteTicketType(int eventId, int id, CancellationToken ct)
    {
        var success = await mediator.Send(new DeleteTicketTypeCommand(id, eventId, 1), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Ticket type not found or unauthorized." });
        return NoContent();
    }
}
