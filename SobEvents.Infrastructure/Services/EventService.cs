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
}
       
        
    
