using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.Events;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Events;

public class DeleteEventCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache)
    : IRequestHandler<DeleteEventCommand, bool>
{
    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await context.Events
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.OrganizerId == request.OrganizerId, cancellationToken);

        if (evt == null) return false;

        evt.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        // Purge cache tag on delete
        await cache.RemoveByTagAsync("events", cancellationToken);

        return true;
    }
}