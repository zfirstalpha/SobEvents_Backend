using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Commands.Reservations;

public record SubmitPaymentProofCommand(int ReservationId, string TransactionReference, int UserId) : IRequest<bool>;

public class SubmitPaymentProofCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<SubmitPaymentProofCommand, bool>
{
    public async Task<bool> Handle(SubmitPaymentProofCommand request, CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == request.UserId, cancellationToken);

        if (reservation == null || reservation.Status == "Cancelled" || reservation.Status == "Confirmed")
        {
            return false;
        }

       
        reservation.TransactionReference = request.TransactionReference;
        reservation.Status = "PendingApproval";
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}