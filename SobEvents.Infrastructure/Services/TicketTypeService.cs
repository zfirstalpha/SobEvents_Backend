using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Data;

namespace SobEvents.Infrastructure.Services;

public class TicketTypeService : ITicketTypeService
{
    private readonly SobEventsDbContext _context;

    public TicketTypeService(SobEventsDbContext context)
    {
        _context = context;
    }

    public async Task<TicketTypeResponseDto?> CreateTicketTypeAsync(int eventId, CreateTicketTypeRequest request, int organizerId)
    {
        // Verify the Event exists AND belongs to this organizer
        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

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
        await _context.SaveChangesAsync();

        return new TicketTypeResponseDto(
            ticketType.Id, ticketType.EventId, ticketType.Name, 
            ticketType.Price, ticketType.Quantity, 
            ticketType.StartDate, ticketType.EndDate, ticketType.IsActive);
    }

    public async Task<List<TicketTypeResponseDto>> GetTicketTypesByEventAsync(int eventId)
    {
        return await _context.TicketTypes
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .Select(t => new TicketTypeResponseDto(
                t.Id, t.EventId, t.Name, t.Price, t.Quantity, 
                t.StartDate, t.EndDate, t.IsActive))
            .ToListAsync();
    }
}