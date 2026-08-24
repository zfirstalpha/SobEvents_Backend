using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries;

namespace SobEvents.Application.Handlers;

public class GetEventByIdQueryHandler(ISobEventsDbContext context) 
    : IRequestHandler<GetEventByIdQuery, EventResponseDto?>
{
    public async Task<EventResponseDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var evt = await context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (evt == null) return null;

        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{evt.Id}", "self", "GET"),
            new($"/api/v1/events/{evt.Id}/tickets", "tickets", "GET"),
            new($"/api/v1/events/{evt.Id}", "update", "PUT"),
            new($"/api/v1/events/{evt.Id}", "delete", "DELETE")
        };

        if (evt.Status == "Draft") links.Add(new($"/api/v1/events/{evt.Id}/publish", "publish", "POST"));
        if (evt.Status == "Published") links.Add(new($"/api/v1/events/{evt.Id}/cancel", "cancel", "POST"));

        return new EventResponseDto(
            evt.Id, evt.Name, evt.Description ?? "", evt.StartDate,
            evt.EndDate, evt.Location, evt.ImageUrl, evt.Status, links);
    }
}