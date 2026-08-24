using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers;

public class UpdateEventCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<UpdateEventCommand, EventResponseDto?>
{
    public async Task<EventResponseDto?> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await context.Events
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.OrganizerId == request.OrganizerId, cancellationToken);

        if (evt == null) return null;

        evt.Name = request.Name;
        evt.Description = request.Description;
        evt.StartDate = request.StartDate;
        evt.EndDate = request.EndDate;
        evt.Location = request.Location;
        evt.ImageUrl = request.ImageUrl;

        await context.SaveChangesAsync(cancellationToken);

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