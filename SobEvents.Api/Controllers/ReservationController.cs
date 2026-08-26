using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Reservations;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Handles ticket reservations, concurrency checks, and booking cancellations using CQRS.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
[EnableRateLimiting("booking-limiter")]
[Authorize] // MODULE 10: All reservation actions require authentication
public class ReservationsController(ISender mediator, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Reserves tickets with real-time capacity checks. Requires logged-in attendee.
    /// </summary>
    [HttpPost("tickets/{ticketTypeId}/reservations")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReservation(int ticketTypeId, [FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var command = new CreateReservationCommand(ticketTypeId, request.Quantity, currentUser.UserId!.Value);
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

    /// <summary>
    /// Retrieves reservation confirmation details.
    /// </summary>
    [HttpGet("reservations/{id}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
    {
        var reservation = await mediator.Send(new GetReservationByIdQuery(id, currentUser.UserId!.Value), ct);
        if (reservation == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found." });
        return Ok(reservation);
    }

    /// <summary>
    /// Retrieves all active and historical reservations for the logged-in attendee.
    /// </summary>
    [HttpGet("reservations/my-reservations")]
    [ProducesResponseType(typeof(List<ReservationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReservations(CancellationToken ct)
    {
        var reservations = await mediator.Send(new GetMyReservationsQuery(currentUser.UserId!.Value), ct);
        return Ok(reservations);
    }

    /// <summary>
    /// Cancels an active reservation.
    /// </summary>
    [HttpDelete("reservations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken ct)
    {
        var success = await mediator.Send(new CancelReservationCommand(id, currentUser.UserId!.Value), ct);
        if (!success) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found or already cancelled." });
        return NoContent();
    }

    /// <summary>
    /// Reclaims abandoned reservations where the 15-minute hold has expired. (Requires Organizer/Admin role).
    /// </summary>
    [HttpPost("reservations/expire-stale")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireStaleReservations(CancellationToken ct)
    {
        var count = await mediator.Send(new ExpireReservationsCommand(), ct);
        return Ok(new { message = $"Successfully cancelled {count} expired reservation(s) and released tickets back to the pool.", count });
    }

    /// <summary>
    /// Enqueues background ticket PDF generation and email delivery.
    /// </summary>
    [HttpPost("reservations/{id}/send-tickets")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendTickets(int id, CancellationToken ct)
    {
        var jobId = await mediator.Send(new QueueTicketDispatchCommand(id, currentUser.UserId!.Value), ct);

        if (jobId == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Reservation not found or has been cancelled." });
        }

        return Accepted(new 
        { 
            message = "Ticket delivery has been queued in the background.", 
            jobId = jobId.Value,
            status = "Processing"
        });
    }
}