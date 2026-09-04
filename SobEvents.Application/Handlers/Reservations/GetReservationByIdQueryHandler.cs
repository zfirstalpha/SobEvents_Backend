using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Reservations;

namespace SobEvents.Application.Handlers.Reservations;

public class GetReservationByIdQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetReservationByIdQuery, ReservationResponseDto?>
{
    public async Task<ReservationResponseDto?> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Include TicketType, Event, and User to populate all DTO fields
        var reservation = await context.Reservations
            .Include(r => r.TicketType)
            .ThenInclude(t => t.Event)
            .Include(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == request.UserId, cancellationToken);

        if (reservation == null) return null;

        var links = new List<LinkDto>
        {
            new($"/api/v1/reservations/{reservation.Id}", "self", "GET")
        };

        if (reservation.Status == "Reserved")
        {
            links.Add(new($"/api/v1/reservations/{reservation.Id}", "cancel", "DELETE"));
            links.Add(new($"/api/v1/reservations/{reservation.Id}/submit-payment", "submit-payment", "POST"));
        }

        // 2. Complete 14-parameter DTO Constructor
        return new ReservationResponseDto(
            reservation.Id,
            reservation.TicketTypeId,
            reservation.TicketType.Name,
            reservation.TicketType.Event.Name,
            reservation.UserId,
            $"{reservation.User.FirstName} {reservation.User.LastName}",
            reservation.User.Email ?? "",
            reservation.Quantity,
            reservation.ReservedAt,
            reservation.ExpiryDate,
            reservation.Status,
            reservation.TransactionReference,
            reservation.RejectionReason,
            links);
    }
}