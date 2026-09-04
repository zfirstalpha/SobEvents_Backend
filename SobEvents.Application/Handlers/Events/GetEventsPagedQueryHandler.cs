using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces; 
using SobEvents.Application.Queries.Events;

namespace SobEvents.Application.Handlers.Events;

public class GetEventsPagedQueryHandler(ISobEventsDbContext context)
    : IRequestHandler<GetEventsPagedQuery, PagedResponseDto<EventResponseDto>>
{
    public async Task<PagedResponseDto<EventResponseDto>> Handle(GetEventsPagedQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;
        var dbQuery = context.Events.AsNoTracking().Where(e=>e.Status == "Published");

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            dbQuery = dbQuery.Where(e => e.Name.ToLower().Contains(search) || e.Location.ToLower().Contains(search));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var events = await dbQuery
            .OrderBy(e => e.StartDate)
            .Skip((request.Page - 1) * request.Pagesize)
            .Take(request.Pagesize)
            .ToListAsync(cancellationToken);

        var dtos = events.Select(e =>
        {
            var links = new List<LinkDto>
            {
                new($"/api/v1/events/{e.Id}", "self", "GET"),
                new($"/api/v1/events/{e.Id}/tickets", "tickets", "GET")
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