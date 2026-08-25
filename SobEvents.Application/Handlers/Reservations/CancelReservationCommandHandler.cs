using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Reservations;

public class CancelReservationCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<CancelReservationCommand, bool>
{
    public async Task<bool> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        // Enforce user ownership
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == request.UserId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled")
        {
            return false;
        }

        reservation.Status = "Cancelled";
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}