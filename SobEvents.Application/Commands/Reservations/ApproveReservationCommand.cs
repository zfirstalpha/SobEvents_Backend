using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Commands.Reservations;

public record ApproveReservationCommand(int ReservationId, int OrganizerId) : IRequest<bool>;

public class ApproveReservationCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<ApproveReservationCommand, bool>
{
    public async Task<bool> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .Include(r => r.TicketType)
            .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.TicketType.Event.OrganizerId == request.OrganizerId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled")
        {
            return false;
        }

        // Lock in the booking permanently
        reservation.Status = "Confirmed";
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}