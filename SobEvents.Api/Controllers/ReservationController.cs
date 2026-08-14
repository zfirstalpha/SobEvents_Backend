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
    public async Task<IActionResult> CreateReservation(int ticketTypeId, [FromBody] CreateReservationRequest request)
    {
        // mocking userId = 1 ....Attendee
        var result = await _reservationService.ReserveTicketsAsync(ticketTypeId, request, 1);

        if (!result.IsSuccess)
        {
            
            return Conflict(new ProblemDetails 
            { 
                Title = "Booking Conflict", 
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        return Created(string.Empty, result.Reservation);
    }
}