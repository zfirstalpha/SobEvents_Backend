using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/tickets")]//nested restufull routing
public class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypesController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

//create a new ticket type for an event
    [HttpPost]
    public async Task<IActionResult> CreateTicketType(int eventId, [FromBody] CreateTicketTypeRequest request, CancellationToken ct = default)
    {
        var ticketType = await _ticketTypeService.CreateTicketTypeAsync(eventId, request, 1,ct); // Mock Organizer Id 1
        
        if (ticketType == null)
        {
            return NotFound(new { message = "Event not found or unauthorized." });
        }

        return CreatedAtAction(nameof(GetTicketTypeById), new { eventId, id = ticketType.Id }, ticketType);
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketTypes(int eventId, CancellationToken ct = default)
    {
        var tickets = await _ticketTypeService.GetTicketTypesByEventAsync(eventId,ct);
        return Ok(tickets);
    }

//get tickettype by id
 [HttpGet("{id}")]
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
     [HttpPut("{id}")]
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
     [HttpDelete("{id}")]
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