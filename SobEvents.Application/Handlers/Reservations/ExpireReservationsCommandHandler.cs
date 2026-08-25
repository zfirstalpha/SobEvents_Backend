using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Reservations;

public class ExpireReservationsCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache)
    : IRequestHandler<ExpireReservationsCommand, int>
{
    public async Task<int> Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        
        // Sends ONE single SQL query directly to PostgreSQL without loading anything into server RAM!
        var rowsUpdated = await context.Reservations
            .Where(r => r.ExpiryDate < now && r.Status == "Reserved")
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(r => r.Status, "Cancelled"),
                cancellationToken
            );

        // If seats were freed up, purge the "tickets" cache tag so the newly available seats show up immediately!
        if (rowsUpdated > 0)
        {
            await cache.RemoveByTagAsync("tickets", cancellationToken);
        }

        return rowsUpdated;
    }
}