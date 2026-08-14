using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Data;

namespace SobEvents.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly SobEventsDbContext _context;

    public ReservationService(SobEventsDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationResult> ReserveTicketsAsync(int ticketTypeId, CreateReservationRequest request, int userId)
    {
        // 1. Fetch the Ticket Type
        var ticketType = await _context.TicketTypes
            .FirstOrDefaultAsync(t => t.Id == ticketTypeId);

        if (ticketType == null || !ticketType.IsActive)
        {
            return new ReservationResult(false, "Ticket type not found or is no longer active.", null);
        }

        // 2. Calculate how many tickets have already been taken (Reserved or Paid)
        var takenTickets = await _context.Reservations
            .Where(r => r.TicketTypeId == ticketTypeId && r.Status != "Cancelled")
            .SumAsync(r => r.Quantity);

        var availableTickets = ticketType.Quantity - takenTickets;

        // 3. BUSINESS RULE: 409 Conflict Check!
        if (request.Quantity > availableTickets)
        {
            return new ReservationResult(false, $"Not enough tickets. Only {availableTickets} left.", null);
        }

        // 4. Create the Reservation (Holds the ticket for 24 hours)
        var reservation = new Reservation
        {
            TicketTypeId = ticketTypeId,
            UserId = userId,
            Quantity = request.Quantity,
            ReservedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddHours(1), // Background Job will use this later!
            Status = "Reserved"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var dto = new ReservationResponseDto(
            reservation.Id, reservation.TicketTypeId, reservation.UserId, 
            reservation.Quantity, reservation.ReservedAt, reservation.ExpiryDate, reservation.Status);

        return new ReservationResult(true, null, dto);
    }
}