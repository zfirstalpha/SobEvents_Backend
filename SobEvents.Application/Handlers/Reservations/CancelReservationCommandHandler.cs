using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Reservations;

public class CancelReservationCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache,
    IEventsHubService hubService) // Injected SignalR broadcaster!
    : IRequestHandler<CancelReservationCommand, bool>
{
    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        // 1. Find reservation including ticket type
        var reservation = await context.Reservations
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == request.UserId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled")
        {
            return false;
        }

        // 2. Cancel the hold
        reservation.Status = "Cancelled";
        await context.SaveChangesAsync(cancellationToken);

        // 3. Purge cache tag
        await cache.RemoveByTagAsync("tickets", cancellationToken);

        // 4. MODULE 9 SESSION 3: Recalculate remaining seats and broadcast live to ALL browsers!
        var takenTickets = await context.Reservations
            .Where(r => r.TicketTypeId == reservation.TicketTypeId && r.Status != "Cancelled")
            .SumAsync(r => r.Quantity, cancellationToken);

        var newAvailable = reservation.TicketType.Quantity - takenTickets;
        await hubService.BroadcastTicketsRemainingUpdatedAsync(reservation.TicketTypeId, newAvailable, cancellationToken);

        return true;
    }
}