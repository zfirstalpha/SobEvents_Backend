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

        return MapToDto(ticketType);
    }

//get ticket by event id
    public async Task<List<TicketTypeResponseDto>> GetTicketTypesByEventAsync(int eventId, CancellationToken ct = default)
    {
        return await _context.TicketTypes
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

// get ticket by id

    public async Task<TicketTypeResponseDto?> GetTicketTypeByIdAsync(int id, CancellationToken ct = default)
    {
        var ticket = await _context.TicketTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return ticket == null ? null : MapToDto(ticket);
    }

     public async Task<TicketTypeResponseDto?> UpdateTicketTypeAsync(int id, UpdateTicketTypeRequest request, int organizerId, CancellationToken ct = default)
    {
        // must belong to an event owned by the organizer
        var ticket = await _context.TicketTypes
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == id && t.Event.OrganizerId == organizerId, ct);

        if (ticket == null) return null;

        ticket.Name = request.Name;
        ticket.Price = request.Price;
        ticket.Quantity = request.Quantity;
        ticket.StartDate = request.StartDate;
        ticket.EndDate = request.EndDate;
        ticket.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);
        return MapToDto(ticket);
    }

    public async Task<bool> DeleteTicketTypeAsync(int id, int organizerId, CancellationToken ct = default)
    {
        var ticket = await _context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == id && t.Event.OrganizerId == organizerId, ct);

        if (ticket == null) return false;

        // BUSINESS DEFENSE: Cannot delete if reservations exist!
        if (ticket.Reservations.Any(r => r.Status != "Cancelled"))
        {
            // Instead of deleting, deactivate it so no more people can buy
            ticket.IsActive = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        _context.TicketTypes.Remove(ticket);
        await _context.SaveChangesAsync(ct);
        return true;
    }

//mapper 
     private static TicketTypeResponseDto MapToDto(TicketType t) =>
        new(t.Id, t.EventId, t.Name, t.Price, t.Quantity, t.StartDate, t.EndDate, t.IsActive);
}