using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Commands.Reservations;

public record RejectReservationCommand(int ReservationId, string? Reason, int OrganizerId) : IRequest<bool>;

public class RejectReservationCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache,
    IEventsHubService hubService)
    : IRequestHandler<RejectReservationCommand, bool>
{
    public async Task<bool> Handle(RejectReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .Include(r => r.TicketType)
            .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.TicketType.Event.OrganizerId == request.OrganizerId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled")
        {
            return false;
        }

        reservation.Status = "Cancelled";
        reservation.RejectionReason = request.Reason ?? "Payment could not be verified.";
        await context.SaveChangesAsync(cancellationToken);

        // Invalidate tickets cache
        await cache.RemoveByTagAsync("tickets", cancellationToken);

        // Recalculate remaining capacity and broadcast SignalR update live!
        var takenTickets = await context.Reservations
            .Where(r => r.TicketTypeId == reservation.TicketTypeId && r.Status != "Cancelled")
            .SumAsync(r => r.Quantity, cancellationToken);

        var newAvailable = reservation.TicketType.Quantity - takenTickets;
        await hubService.BroadcastTicketsRemainingUpdatedAsync(reservation.TicketTypeId, newAvailable, cancellationToken);

        return true;
    }
}