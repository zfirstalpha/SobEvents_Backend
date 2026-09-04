using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Events;

namespace SobEvents.Application.Handlers.Events;

public class GetOrganizerEventsQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetOrganizerEventsQuery, PagedResponseDto<EventResponseDto>>
{
    public async Task<PagedResponseDto<EventResponseDto>> Handle(GetOrganizerEventsQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        //  Strict Organizer Isolation: Filter where OrganizerId matches caller!
        var dbQuery = context.Events
            .Include(e => e.TicketTypes)
            .AsNoTracking()
            .Where(e => e.OrganizerId == query.OrganizerId);

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            dbQuery = dbQuery.Where(e => e.Name.ToLower().Contains(search) || e.Location.ToLower().Contains(search));
        }

        // 3. Count
        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // 4. Sort (Newest first) & Paginate
        var events = await dbQuery
            .OrderByDescending(e => e.StartDate)
            .Skip((request.Page - 1) * request.Pagesize)
            .Take(request.Pagesize)
            .ToListAsync(cancellationToken);

        // 5. Map HATEOAS links
        var dtos = events.Select(e =>
        {
            var links = new List<LinkDto>
            {
                new($"/api/v1/events/{e.Id}", "self", "GET"),
                new($"/api/v1/events/{e.Id}/tickets", "tickets", "GET"),
                new($"/api/v1/events/{e.Id}", "update", "PUT"),
                new($"/api/v1/events/{e.Id}", "delete", "DELETE")
            };

            if (e.Status == "Draft") links.Add(new($"/api/v1/events/{e.Id}/publish", "publish", "POST"));
            if (e.Status == "Published") links.Add(new($"/api/v1/events/{e.Id}/cancel", "cancel", "POST"));

            return new EventResponseDto(
                e.Id, e.Name, e.Description ?? "", e.StartDate, e.EndDate,
                e.Location, e.ImageUrl, e.Status, links);
        }).ToList();

        return new PagedResponseDto<EventResponseDto>(dtos, totalCount, request.Page, request.Pagesize);
    }
}