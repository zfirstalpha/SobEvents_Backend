using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Reservations;

public class QueueTicketDispatchCommandHandler(
    ISobEventsDbContext context,
    ITicketJobQueue queue) 
    : IRequestHandler<QueueTicketDispatchCommand, Guid?>
{
    public async Task<Guid?> Handle(QueueTicketDispatchCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch reservation including ticket type and event details
        var reservation = await context.Reservations
            .Include(r => r.TicketType)
            .ThenInclude(t => t.Event)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == request.UserId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled")
        {
            return null;
        }

        var jobId = Guid.NewGuid();

        var job = new TicketJob(
            JobId: jobId,
            ReservationId: reservation.Id,
            UserEmail: reservation.User.Email,
            EventName: reservation.TicketType.Event.Name,
            Quantity: reservation.Quantity,
            QueuedAt: DateTime.UtcNow
        );

        // 2. Enqueue into non-blocking memory Channel
        await queue.EnqueueAsync(job, cancellationToken);

        return jobId;
    }
}