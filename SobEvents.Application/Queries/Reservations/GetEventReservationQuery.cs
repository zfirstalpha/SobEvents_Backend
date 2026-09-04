using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Queries.Reservations;

public record GetEventReservationsQuery(int EventId, int OrganizerId) : IRequest<List<ReservationResponseDto>>;

public class GetEventReservationsQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetEventReservationsQuery, List<ReservationResponseDto>>
{
    public async Task<List<ReservationResponseDto>> Handle(GetEventReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await context.Reservations
            .Include(r => r.TicketType)
            .ThenInclude(t => t.Event)
            .Include(r => r.User)
            .AsNoTracking()
            .Where(r => r.TicketType.EventId == request.EventId && r.TicketType.Event.OrganizerId == request.OrganizerId)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync(cancellationToken);

        return reservations.Select(r => new ReservationResponseDto(
            r.Id,
            r.TicketTypeId,
            r.TicketType.Name,
            r.TicketType.Event.Name,
            r.UserId,
            $"{r.User.FirstName} {r.User.LastName}",
            r.User.Email ?? "",
            r.Quantity,
            r.ReservedAt,
            r.ExpiryDate,
            r.Status,
            r.TransactionReference,
            r.RejectionReason,
            new List<LinkDto>
            {
                new($"/api/v1/reservations/{r.Id}", "self", "GET"),
                new($"/api/v1/reservations/{r.Id}/approve", "approve", "POST"),
                new($"/api/v1/reservations/{r.Id}/reject", "reject", "POST")
            }
        )).ToList();
    }
}