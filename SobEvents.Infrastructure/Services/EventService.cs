using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Data;

namespace SobEvents.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly SobEventsDbContext _context;
    public EventService(SobEventsDbContext context)
    {
        _context = context;
    }

    public async Task<EventResponseDto> CreateEventAsync(CreateEventRequest request, int organizerId)
    {
        //map dto to entity
        var newEvent = new Event
        {
            Name = request.Name,
            Description = request.Description,
            Date = request.Date,
            Location = request.Location,
            OrganizerId = organizerId,
            Status = "Published"

        };
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        // map entity back to dto to return to the frontend
        return new EventResponseDto(
            newEvent.Id,
            newEvent.Name,
            newEvent.Description,
            newEvent.Date,
            newEvent.Location,
            newEvent.Status);
    }

    // get all events
    public async Task<List<EventResponseDto>> GetAllEventsAsync()
    {
        return await _context.Events
        .AsNoTracking()
        .Select(e=> new EventResponseDto(
            e.Id,
            e.Name,
            e.Description,
            e.Date,
            e.Location,
            e.Status
        ))
        .ToListAsync();
        
    }
}
       
        
    
