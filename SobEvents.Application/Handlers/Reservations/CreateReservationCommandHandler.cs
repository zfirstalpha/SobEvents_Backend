using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands.Reservations;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.Reservations;

public class CreateReservationCommandHandler(ISobEventsDbContext context,HybridCache cache)
    : IRequestHandler<CreateReservationCommand, ReservationResult>
{
    public async Task<ReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Ticket Type
        var ticketType = await context.TicketTypes
            .FirstOrDefaultAsync(t => t.Id == request.TicketTypeId, cancellationToken);

        if (ticketType == null || !ticketType.IsActive)
        {
            return new ReservationResult(false, "Ticket type not found or is no longer active.", null);
        }

        // 2. Concurrency & Capacity check
        var takenTickets = await context.Reservations
            .Where(r => r.TicketTypeId == request.TicketTypeId && r.Status != "Cancelled")
            .SumAsync(r => r.Quantity, cancellationToken);

        var availableTickets = ticketType.Quantity - takenTickets;

        // 3. Business Rule: 409 Conflict Check
        if (request.Quantity > availableTickets)
        {
            return new ReservationResult(false, $"Not enough tickets available. Only {availableTickets} remaining.", null);
        }

        // 4. Create the Reservation with 24-hour expiration
        var reservation = new Reservation
        {
            TicketTypeId = request.TicketTypeId,
            UserId = request.UserId,
            Quantity = request.Quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            Status = "Reserved"
        };
            
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync("tickets", cancellationToken);

        var links = new List<LinkDto>
        {
            new($"/api/v1/reservations/{reservation.Id}", "self", "GET"),
            new($"/api/v1/reservations/{reservation.Id}", "cancel", "DELETE")
        };

        var dto = new ReservationResponseDto(
            reservation.Id, reservation.TicketTypeId, reservation.UserId,
            reservation.Quantity, reservation.ReservedAt, reservation.ExpiryDate,
            reservation.Status, links);

        return new ReservationResult(true, null, dto);
    }
}