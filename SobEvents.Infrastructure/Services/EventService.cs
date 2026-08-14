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


//map entity to dto
    private static EventResponseDto MapToDto(Event e)
    {
        return new EventResponseDto(
            e.Id,
            e.Name,
            e.Description??"",
            e.StartDate,
            e.EndDate,
            e.Location,
            e.ImageUrl,
            e.Status
        );
    }


//create event
    public async Task<EventResponseDto> CreateEventAsync(CreateEventRequest request, int organizerId)
    {
        //map dto to entity
        var newEvent = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            ImageUrl = request.ImageUrl,
            OrganizerId = organizerId,
            Status = "Draft"

        };
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        // map entity back to dto to return to the frontend
        return MapToDto(newEvent);
    }

// get all events
    public async Task<PagedResponseDto<EventResponseDto>> GetAllEventsAsync(PagedRequestDto request)
    {
        // start the query asnotracking for readonly 
        var query = _context.Events.AsNoTracking();

        //search by name or location
        if(!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(e=> e.Name.ToLower().Contains(searchTerm) 
            || e.Location.ToLower().Contains(searchTerm)
            );
        }
        //count before skip and take
    var totalCount = await query.CountAsync();
    
    //sort and paginate (skip and take)
    var items = await query
    .OrderBy(e => e.StartDate)
    .Skip((request.Page - 1) * request.Pagesize)
    .Take(request.Pagesize)
    .Select(e => MapToDto(e))
    .ToListAsync();
      return new PagedResponseDto<EventResponseDto>(items, totalCount, request.Page, request.Pagesize);  
    }
//get event by id
    public async Task<EventResponseDto?>
    GetEventByIdAsync(int id)
    {
        var evt = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if(evt == null)
        {
            return null;
        }
        return MapToDto(evt);
    }
//update event
    public async Task<EventResponseDto?> UpdateEventAsync(int id, CreateEventRequest request, int organizerId)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);
        if(evt == null)
        {
            return null;
        }
        evt.Name = request.Name;
        evt.Description = request.Description;
        evt.StartDate = request.StartDate;
        evt.EndDate = request.EndDate;
        evt.Location = request.Location;
        evt.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync();
        return MapToDto(evt);
    }

//delete event
    public async Task<bool> DeleteEventAsync(int id, int organizerId)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);
        if(evt == null)
        {
            return false;
        }
        evt.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
       
        
    
