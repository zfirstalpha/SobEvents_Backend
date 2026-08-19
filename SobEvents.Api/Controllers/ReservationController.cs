using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketTypeId}/reservations")] // Nested routing
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
//create reservagtion

    [HttpPost]
    public async Task<IActionResult> CreateReservation(int ticketTypeId, [FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        // mocking userId = 1 ....Attendee
        var result = await _reservationService.ReserveTicketsAsync(ticketTypeId, request, 1,ct);

        if (!result.IsSuccess)
        {
            
            return Conflict(new ProblemDetails 
            { 
                Title = "Booking Conflict", 
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        // REST 201 + location header pointing to GetReservationById
        return CreatedAtAction(nameof(GetReservationById), new { id = result.Reservation!.Id }, result.Reservation);
    }

 // get reservation detail by id
    [HttpGet("api/reservations/{id}")]
    public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id, 1, ct); // Mock Attendee Id 1
        
        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found." });
        }

        return Ok(reservation);
    }

// Attendee personal booking list
    [HttpGet("api/reservations/my-reservations")]
    public async Task<IActionResult> GetMyReservations(CancellationToken ct)
    {
        var reservations = await _reservationService.GetReservationsByUserAsync(1, ct); // Mock Attendee Id 1
        return Ok(reservations);
    }

//reservation cancelation 
    
    [HttpDelete("api/reservations/{id}")]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken ct)
    {
        var success = await _reservationService.CancelReservationAsync(id, 1, ct); // Mock Attendee Id 1
        
        if (!success)
        {
            return NotFound(new { message = "Reservation not found or already cancelled." });
        }

        return NoContent(); // 204 No Content
    }

}