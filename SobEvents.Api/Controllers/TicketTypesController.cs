using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Manages ticket tiers (VIP, General) and capacity limits for events.
/// </summary>
[ApiController]
[Route("api/events/{eventId}/tickets")]//nested restufull routing
[Produces("application/json")]
public class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypesController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

//create a new ticket type for an event

    /// <summary>
    /// Creates a new ticket tier for an event.
    /// </summary>
    
    [HttpPost]
    [ProducesResponseType(typeof(TicketTypeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTicketType(int eventId, [FromBody] CreateTicketTypeRequest request, CancellationToken ct = default)
    {
        var ticketType = await _ticketTypeService.CreateTicketTypeAsync(eventId, request, 1,ct); // Mock Organizer Id 1
        
        if (ticketType == null)
        {
            return NotFound(new { message = "Event not found or unauthorized." });
        }

        return CreatedAtAction(nameof(GetTicketTypeById), new { eventId, id = ticketType.Id }, ticketType);
    }

    
//get ticket types

        /// <summary>
    /// Retrieves all ticket tiers for an event with real-time remaining capacity.
    /// </summary>

[HttpGet]
[ProducesResponseType(typeof(List<TicketTypeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketTypes(int eventId, CancellationToken ct = default)
    {
        var tickets = await _ticketTypeService.GetTicketTypesByEventAsync(eventId,ct);
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
        var ticket = await _ticketTypeService.GetTicketTypeByIdAsync(id, ct);
        if (ticket == null || ticket.EventId != eventId)
        {
            return NotFound(new { message = "Ticket type not found." });
        }
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
        var updated = await _ticketTypeService.UpdateTicketTypeAsync(id, request, 1, ct);
        if (updated == null)
        {
            return NotFound(new { message = "Ticket type not found or unauthorized." });
        }
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
        var success = await _ticketTypeService.DeleteTicketTypeAsync(id, 1, ct);
        if (!success)
        {
            return NotFound(new { message = "Ticket type not found or unauthorized." });
        }
        return NoContent(); // 204 No Content
    }
}