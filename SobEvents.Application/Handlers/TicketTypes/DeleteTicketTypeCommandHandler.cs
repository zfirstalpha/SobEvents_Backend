using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands.TicketTypes;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.TicketTypes;

public class DeleteTicketTypeCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<DeleteTicketTypeCommand, bool>
{
    public async Task<bool> Handle(DeleteTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var ticket = await context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.EventId == request.EventId && t.Event.OrganizerId == request.OrganizerId, cancellationToken);

        if (ticket == null) return false;

        // Enterprise Defense: Deactivate if reservations exist
        if (ticket.Reservations.Any(r => r.Status != "Cancelled"))
        {
            ticket.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        context.TicketTypes.Remove(ticket);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}