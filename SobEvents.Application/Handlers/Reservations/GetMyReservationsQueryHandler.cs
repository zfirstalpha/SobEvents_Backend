using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Reservations;

namespace SobEvents.Application.Handlers.Reservations;

public class GetMyReservationsQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetMyReservationsQuery, List<ReservationResponseDto>>
{
    public async Task<List<ReservationResponseDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await context.Reservations
            .AsNoTracking()
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync(cancellationToken);

        return reservations.Select(r =>
        {
            var links = new List<LinkDto>
            {
                new($"/api/v1/reservations/{r.Id}", "self", "GET")
            };

            if (r.Status == "Reserved")
            {
                links.Add(new($"/api/v1/reservations/{r.Id}", "cancel", "DELETE"));
            }

            return new ReservationResponseDto(
                r.Id, r.TicketTypeId, r.UserId, r.Quantity,
                r.ReservedAt, r.ExpiryDate, r.Status, links);
        }).ToList();
    }
}