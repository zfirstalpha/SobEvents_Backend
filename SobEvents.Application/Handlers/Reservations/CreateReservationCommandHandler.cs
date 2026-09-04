using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.Reservations;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.Reservations;

public class CreateReservationCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache,
    IEventsHubService hubService)
    : IRequestHandler<CreateReservationCommand, ReservationResult>
{
    public async Task<ReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var ticketType = await context.TicketTypes
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == request.TicketTypeId, cancellationToken);

        if (ticketType == null || !ticketType.IsActive)
        {
            return new ReservationResult(false, "Ticket type not found or is no longer active.", null);
        }

        var takenTickets = await context.Reservations
            .Where(r => r.TicketTypeId == request.TicketTypeId && r.Status != "Cancelled")
            .SumAsync(r => r.Quantity, cancellationToken);

        var availableTickets = ticketType.Quantity - takenTickets;

        if (request.Quantity > availableTickets)
        {
            return new ReservationResult(false, $"Not enough tickets available. Only {availableTickets} remaining.", null);
        }

        var reservation = new Reservation
        {
            TicketTypeId = request.TicketTypeId,
            UserId = request.UserId,
            Quantity = request.Quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMinutes(15),
            Status = "Reserved"
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync("tickets", cancellationToken);

        var newRemaining = availableTickets - request.Quantity;
        await hubService.BroadcastTicketsRemainingUpdatedAsync(ticketType.Id, newRemaining, cancellationToken);

        var links = new List<LinkDto>
        {
            new($"/api/v1/reservations/{reservation.Id}", "self", "GET"),
            new($"/api/v1/reservations/{reservation.Id}", "cancel", "DELETE"),
            new($"/api/v1/reservations/{reservation.Id}/submit-payment", "submit-payment", "POST")
        };

        var dto = new ReservationResponseDto(
            reservation.Id,
            reservation.TicketTypeId,
            ticketType.Name,
            ticketType.Event.Name,
            reservation.UserId,
            "", "", // User names loaded on Get queries
            reservation.Quantity,
            reservation.ReservedAt,
            reservation.ExpiryDate,
            reservation.Status,
            null,
            null,
            links);

        return new ReservationResult(true, null, dto);
    }
}