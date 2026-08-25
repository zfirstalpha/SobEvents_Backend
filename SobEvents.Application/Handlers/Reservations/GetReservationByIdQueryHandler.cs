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
        var reservation = await context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == request.UserId, cancellationToken);

        if (reservation == null) return null;

        var links = new List<LinkDto>
        {
            new($"/api/v1/reservations/{reservation.Id}", "self", "GET")
        };

        // CONDITIONAL LINK: Only emit cancel if not already cancelled
        if (reservation.Status == "Reserved")
        {
            links.Add(new($"/api/v1/reservations/{reservation.Id}", "cancel", "DELETE"));
        }

        return new ReservationResponseDto(
            reservation.Id, reservation.TicketTypeId, reservation.UserId,
            reservation.Quantity, reservation.ReservedAt, reservation.ExpiryDate,
            reservation.Status, links);
    }
}