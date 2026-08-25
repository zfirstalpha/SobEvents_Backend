using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using Asp.Versioning;
using MediatR;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.Queries.Reservations;
using Microsoft.AspNetCore.RateLimiting;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Handles ticket reservations, concurrency checks, and booking cancellations.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}")] // Nested routing
[Produces("application/json")]
[EnableRateLimiting("general-limiter")] 
public class ReservationsController(ISender mediator) : ControllerBase
{
//create reservagtion

  /// <summary>
    /// Reserves tickets with real-time capacity and concurrency conflict checks.
    /// </summary>
    [HttpPost("tickets/{ticketTypeId}/reservations")]
      [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation(int ticketTypeId, [FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var command = new CreateReservationCommand(ticketTypeId, request.Quantity, 1); // Mock Attendee Id 1
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return Conflict(new ProblemDetails 
            { 
                Title = "Booking Conflict", 
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        return CreatedAtAction(nameof(GetReservationById), new { id = result.Reservation!.Id }, result.Reservation);
    }
 // get reservation detail by id

 /// <summary>
    /// Retrieves reservation confirmation details.
    /// </summary>
    [HttpGet("reservations/{id}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
    {
        var reservation = await mediator.Send(new GetReservationByIdQuery(id, 1), ct);
        if (reservation == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found." });
        return Ok(reservation);
    }

// Attendee personal booking list

/// <summary>
    /// Retrieves all active and historical reservations for the logged-in attendee.
    /// </summary>

    [HttpGet("reservations/my-reservations")]
    [ProducesResponseType(typeof(List<ReservationResponseDto>), StatusCodes.Status200OK)]
    
    public async Task<IActionResult> GetMyReservations(CancellationToken ct)
    {
        var reservations = await mediator.Send(new GetMyReservationsQuery(1), ct);
        return Ok(reservations);
    }

//reservation cancelation 

  /// <summary>
    /// Cancels an active reservation and releases ticket capacity back to the pool.
    /// </summary>
    
    [HttpDelete("reservations/{id}")]
      [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken ct)
    {
        var success = await mediator.Send(new CancelReservationCommand(id, 1), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found or already cancelled." });
        return NoContent();
    }
//reservation expire
/// <summary>
    /// Reclaims abandoned reservations where the 15-minute hold has expired.
    /// </summary>
    [HttpPost("reservations/expire-stale")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireStaleReservations(CancellationToken ct)
    {
        var count = await mediator.Send(new ExpireReservationsCommand(), ct);
        return Ok(new { message = $"Successfully cancelled {count} expired reservation(s) and released tickets back to the pool.", count });
    }

/// <summary>
    /// Enqueues background ticket PDF generation and email delivery. Returns immediately with 202 Accepted.
    /// </summary>
    [HttpPost("reservations/{id}/send-tickets")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendTickets(int id, CancellationToken ct)
    {
        var jobId = await mediator.Send(new QueueTicketDispatchCommand(id, 1), ct); // Mock Attendee Id 1

        if (jobId == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found or has been cancelled." });
        }

        // 202 Accepted indicates work has been queued for background execution
        return Accepted(new 
        { 
            message = "Ticket delivery has been queued in the background.", 
            jobId = jobId.Value,
            status = "Processing"
        });
    }
}