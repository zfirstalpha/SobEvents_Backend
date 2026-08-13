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
    .OrderBy(e => e.Date)
    .Skip((request.Page - 1) * request.Pagesize)
    .Take(request.Pagesize)
    .Select(e => new EventResponseDto(
        e.Id,
        e.Name,
        e.Description,
        e.Date,
        e.Location,
        e.Status
    ))
    .ToListAsync();
      return new PagedResponseDto<EventResponseDto>(items, totalCount, request.Page, request.Pagesize);  
    }

}
       
        
    
