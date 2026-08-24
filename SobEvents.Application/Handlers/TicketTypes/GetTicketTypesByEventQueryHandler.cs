using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.TicketTypes;

namespace SobEvents.Application.Handlers.TicketTypes;

public class GetTicketTypesByEventQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetTicketTypesByEventQuery, List<TicketTypeResponseDto>>
{
    public async Task<List<TicketTypeResponseDto>> Handle(GetTicketTypesByEventQuery request, CancellationToken cancellationToken)
    {
        var tickets = await context.TicketTypes
            .Include(t => t.Reservations)
            .AsNoTracking()
            .Where(t => t.EventId == request.EventId)
            .ToListAsync(cancellationToken);

        return tickets.Select(t =>
        {
            var reserved = t.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
            var available = t.Quantity - reserved;

            var links = new List<LinkDto>
            {
                new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "self", "GET"),
                new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "update", "PUT"),
                new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "delete", "DELETE")
            };

            // CONDITIONAL HATEOAS LINK (Module 6 Slide 10)
            if (available > 0 && t.IsActive)
            {
                links.Add(new($"/api/v1/tickets/{t.Id}/reservations", "reserve", "POST"));
            }

            return new TicketTypeResponseDto(
                t.Id, t.EventId, t.Name, t.Price, t.Quantity, available,
                t.StartDate, t.EndDate, t.IsActive, links);
        }).ToList();
    }
}