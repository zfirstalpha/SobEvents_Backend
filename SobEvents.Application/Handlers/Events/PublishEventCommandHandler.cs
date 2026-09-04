using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.Events;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Events;

public class PublishEventCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache,
    IEventsHubService hubService) // Pure Application Interface!
    : IRequestHandler<PublishEventCommand, PublishEventResult>
{
    public async Task<PublishEventResult> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await context.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.OrganizerId == request.OrganizerId, cancellationToken);

        if (evt == null) return new PublishEventResult(false, "Event not found or unauthorized.");
        if (evt.Status == "Published") return new PublishEventResult(false, "Event is already published.");
        if (!evt.TicketTypes.Any(t => t.IsActive)) return new PublishEventResult(false, "Cannot publish an event without at least one active ticket type.");

        evt.Status = "Published";
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("events", cancellationToken);

        // Broadcast status change in real time
        await hubService.BroadcastEventStatusChangedAsync(evt.Id, "Published", cancellationToken);

        return new PublishEventResult(true, null);
    }
}