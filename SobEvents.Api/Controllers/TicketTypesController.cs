using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/tickets")] // Nested RESTful routing!
public class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypesController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

//create a new ticket type for an event
    [HttpPost]
    public async Task<IActionResult> CreateTicketType(int eventId, [FromBody] CreateTicketTypeRequest request)
    {
        var ticketType = await _ticketTypeService.CreateTicketTypeAsync(eventId, request, 1); // Mock Organizer Id 1
        
        if (ticketType == null)
        {
            return NotFound(new { message = "Event not found or unauthorized." });
        }

        return Created(string.Empty, ticketType); // 201 Created
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketTypes(int eventId)
    {
        var tickets = await _ticketTypeService.GetTicketTypesByEventAsync(eventId);
        return Ok(tickets);
    }
}