using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Events;

namespace SobEvents.Application.Handlers.Events;

public class GetEventByIdQueryHandler(
    ISobEventsDbContext context,
    HybridCache cache) 
    : IRequestHandler<GetEventByIdQuery, EventResponseDto?>
{
    public async Task<EventResponseDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"event-{request.Id}";

        // HybridCache with stampede protection & tag invalidation
        return await cache.GetOrCreateAsync(
            cacheKey,
            async cancelToken =>
            {
                var evt = await context.Events
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancelToken);

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
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5), // Cache for 5 minutes
                LocalCacheExpiration = TimeSpan.FromMinutes(2) // L1 RAM cache for 2 minutes
            },
            tags: ["events"], // Tag for instant invalidation on mutation!
            cancellationToken: cancellationToken
        );
    }
}