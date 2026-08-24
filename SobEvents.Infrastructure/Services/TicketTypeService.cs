using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Persistence.Context;

namespace SobEvents.Infrastructure.Services;

public class TicketTypeService : ITicketTypeService
{
    private readonly SobEventsDbContext _context;

    public TicketTypeService(SobEventsDbContext context)
    {
        _context = context;
    }

//create ticket type
    public async Task<TicketTypeResponseDto?> CreateTicketTypeAsync(int eventId, CreateTicketTypeRequest request, int organizerId, CancellationToken ct = default)
    {
        // Verify the Event exists AND belongs to this organizer
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId && e.OrganizerId == organizerId,ct);

        if (!eventExists) return null; 

        // 2. Map DTO to Entity
        var ticketType = new TicketType
        {
            EventId = eventId,
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        _context.TicketTypes.Add(ticketType);
        await _context.SaveChangesAsync(ct);

        return MapToDto(ticketType,ticketType.Quantity);
    }

//get ticket by event id
    public async Task<List<TicketTypeResponseDto>> GetTicketTypesByEventAsync(int eventId, CancellationToken ct = default)
    {
        var tickets = await _context.TicketTypes
            .Include(t => t.Reservations)
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .ToListAsync(ct);

     return tickets.Select(t =>
        {
            var reservedCount = t.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
            var available = t.Quantity - reservedCount;
            return MapToDto(t, available);
        }).ToList();
    }

// get ticket by id

    public async Task<TicketTypeResponseDto?> GetTicketTypeByIdAsync(int id, CancellationToken ct = default)
    {
        var ticket = await _context.TicketTypes
            .Include(t => t.Reservations)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (ticket == null) return null;

        var reservedCount = ticket.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
        var available = ticket.Quantity - reservedCount;
        return MapToDto(ticket, available);
    }

//update ticket type
     public async Task<TicketTypeResponseDto?> UpdateTicketTypeAsync(int id, UpdateTicketTypeRequest request, int organizerId, CancellationToken ct = default)
    {
        var ticket = await _context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == id && t.Event.OrganizerId == organizerId, ct);

        if (ticket == null) return null;

        ticket.Name = request.Name;
        ticket.Price = request.Price;
        ticket.Quantity = request.Quantity;
        ticket.StartDate = request.StartDate;
        ticket.EndDate = request.EndDate;
        ticket.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        var reservedCount = ticket.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
        var available = ticket.Quantity - reservedCount;
        return MapToDto(ticket, available);
    }

    public async Task<bool> DeleteTicketTypeAsync(int id, int organizerId, CancellationToken ct = default)
    {
         var ticket = await _context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == id && t.Event.OrganizerId == organizerId, ct);

        if (ticket == null) return false;

        if (ticket.Reservations.Any(r => r.Status != "Cancelled"))
        {
            ticket.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        _context.TicketTypes.Remove(ticket);
        await _context.SaveChangesAsync(ct);
        return true;
        }

       
    

//mapper 
      private static TicketTypeResponseDto MapToDto(TicketType t, int availableQuantity)
    {
        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "self", "GET"),
new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "update", "PUT"),
new($"/api/v1/events/{t.EventId}/tickets/{t.Id}", "delete", "DELETE"),
new($"/api/v1/tickets/{t.Id}/reservations", "reserve", "POST")
        };

        // CONDITIONAL LINK: Only emit "reserve" if seats are available!
        if (availableQuantity > 0 && t.IsActive)
        {
            links.Add(new($"/api/tickets/{t.Id}/reservations", "reserve", "POST"));
        }

        return new TicketTypeResponseDto(
            t.Id, t.EventId, t.Name, t.Price, t.Quantity, availableQuantity,
            t.StartDate, t.EndDate, t.IsActive, links);
    }
}