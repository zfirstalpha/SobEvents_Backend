using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Handles ticket reservations, concurrency checks, and booking cancellations.
/// </summary>
[ApiController]
[Route("api/tickets/{ticketTypeId}/reservations")] // Nested routing
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
//create reservagtion

  /// <summary>
    /// Reserves tickets with real-time capacity and concurrency conflict checks.
    /// </summary>
    [HttpPost]
      [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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

 /// <summary>
    /// Retrieves reservation confirmation details.
    /// </summary>
    [HttpGet("api/reservations/{id}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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

/// <summary>
    /// Retrieves all active and historical reservations for the logged-in attendee.
    /// </summary>

    [HttpGet("api/reservations/my-reservations")]
    [ProducesResponseType(typeof(List<ReservationResponseDto>), StatusCodes.Status200OK)]
    
    public async Task<IActionResult> GetMyReservations(CancellationToken ct)
    {
        var reservations = await _reservationService.GetReservationsByUserAsync(1, ct); // Mock Attendee Id 1
        return Ok(reservations);
    }

//reservation cancelation 

  /// <summary>
    /// Cancels an active reservation and releases ticket capacity back to the pool.
    /// </summary>
    
    [HttpDelete("api/reservations/{id}")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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