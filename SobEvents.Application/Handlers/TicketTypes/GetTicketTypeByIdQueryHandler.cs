using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.TicketTypes;

namespace SobEvents.Application.Handlers.TicketTypes;

public class GetTicketTypeByIdQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetTicketTypeByIdQuery, TicketTypeResponseDto?>
{
    public async Task<TicketTypeResponseDto?> Handle(GetTicketTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await context.TicketTypes
            .Include(t => t.Reservations)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.EventId == request.EventId, cancellationToken);

        if (ticket == null) return null;

        var reserved = ticket.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
        var available = ticket.Quantity - reserved;

        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "self", "GET"),
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "update", "PUT"),
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "delete", "DELETE")
        };

        if (available > 0 && ticket.IsActive)
        {
            links.Add(new($"/api/v1/tickets/{ticket.Id}/reservations", "reserve", "POST"));
        }

        return new TicketTypeResponseDto(
            ticket.Id, ticket.EventId, ticket.Name, ticket.Price, ticket.Quantity, available,
            ticket.StartDate, ticket.EndDate, ticket.IsActive, links);
    }
}