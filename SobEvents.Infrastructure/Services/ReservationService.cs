// using Microsoft.EntityFrameworkCore;
// using SobEvents.Application.DTOs;
// using SobEvents.Application.Interfaces;
// using SobEvents.Domain.Entities;
// using SobEvents.Infrastructure.Persistence.Context;

// namespace SobEvents.Infrastructure.Services;

// public class ReservationService : IReservationService
// {
//     private readonly SobEventsDbContext _context;

//     public ReservationService(SobEventsDbContext context)
//     {
//         _context = context;
//     }
// //reserve ticket
//     public async Task<ReservationResult> ReserveTicketsAsync(int ticketTypeId, CreateReservationRequest request, int userId,CancellationToken ct =default)
//     {
//         //  fetch ticket type
//         var ticketType = await _context.TicketTypes
//             .FirstOrDefaultAsync(t => t.Id == ticketTypeId,ct);

//         if (ticketType == null || !ticketType.IsActive)
//         {
//             return new ReservationResult(false, "Ticket type not found or is no longer active.", null);
//         }

//         // calculate how many tickets reserved or paid
//         var takenTickets = await _context.Reservations
//             .Where(r => r.TicketTypeId == ticketTypeId && r.Status != "Cancelled")
//             .SumAsync(r => r.Quantity,ct);

//         var availableTickets = ticketType.Quantity - takenTickets;

//         // 3. business rule 409 conflict check
//         if (request.Quantity > availableTickets)
//         {
//             return new ReservationResult(false, $"Not enough tickets. Only {availableTickets} remaining.", null);
//         }

//         // create the reservation (holds the ticket for  hours)
//         var reservation = new Reservation
//         {
//             TicketTypeId = ticketTypeId,
//             UserId = userId,
//             Quantity = request.Quantity,
//             ReservedAt = DateTime.UtcNow,
//             ExpiryDate = DateTime.UtcNow.AddHours(24), // Background job i will use this later!
//             Status = "Reserved"
//         };

//         _context.Reservations.Add(reservation);
//         await _context.SaveChangesAsync(ct);

//         return new ReservationResult(true, null, MapToDto(reservation));
//     }
// //get reservation by id
//     public async Task<ReservationResponseDto?> GetReservationByIdAsync(int id, int userId, CancellationToken ct = default)
//     {
//         var reservation = await _context.Reservations
//             .AsNoTracking()
//             .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct);

//         return reservation == null ? null : MapToDto(reservation);
//     }
// //get reservation by user id 
//      public async Task<List<ReservationResponseDto>> GetReservationsByUserAsync(int userId, CancellationToken ct = default)
//     {
//         return await _context.Reservations
//             .AsNoTracking()
//             .Where(r => r.UserId == userId)
//             .OrderByDescending(r => r.ReservedAt)
//             .Select(r => MapToDto(r))
//             .ToListAsync(ct);
//     }

// //cancel reservation 
//      public async Task<bool> CancelReservationAsync(int id, int userId, CancellationToken ct = default)
//     {
//         var reservation = await _context.Reservations
//             .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct);

//         if (reservation == null || reservation.Status == "Cancelled")
//         {
//             return false;
//         }

//         // marked Cancelled so the capacity is automatically freed for other attendees
//         reservation.Status = "Cancelled";
//         await _context.SaveChangesAsync(ct);

//         return true;
//     }

// //map to dto with hateoas
//      private static ReservationResponseDto MapToDto(Reservation r)
//     {
//         var links = new List<LinkDto>
//         {
//             new($"/api/v1/reservations/{r.Id}", "self", "GET"),
// new($"/api/v1/reservations/{r.Id}", "cancel", "DELETE")
//         };

//         // CONDITIONAL LINK: Can only cancel if active
//         if (r.Status == "Reserved")
//         {
//             links.Add(new($"/api/reservations/{r.Id}", "cancel", "DELETE"));
//         }

//         return new ReservationResponseDto(
//             r.Id, r.TicketTypeId, r.UserId, r.Quantity, r.ReservedAt, r.ExpiryDate, r.Status, links);
//     }
// }