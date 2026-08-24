using MediatR;
using SobEvents.Application.Commands;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces; // Pure Application interface!
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers;

public class CreateEventCommandHandler(ISobEventsDbContext context) 
    : IRequestHandler<CreateEventCommand, EventResponseDto>
{
    public async Task<EventResponseDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var newEvent = new Event
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            ImageUrl = request.ImageUrl,
            OrganizerId = request.OrganizerId,
            Status = "Draft"
        };

        context.Events.Add(newEvent);
        await context.SaveChangesAsync(cancellationToken);

        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{newEvent.Id}", "self", "GET"),
            new($"/api/v1/events/{newEvent.Id}/tickets", "tickets", "GET"),
            new($"/api/v1/events/{newEvent.Id}", "update", "PUT"),
            new($"/api/v1/events/{newEvent.Id}", "delete", "DELETE"),
            new($"/api/v1/events/{newEvent.Id}/publish", "publish", "POST")
        };

        return new EventResponseDto(
            newEvent.Id, newEvent.Name, newEvent.Description ?? "",
            newEvent.StartDate, newEvent.EndDate, newEvent.Location,
            newEvent.ImageUrl, newEvent.Status, links);
    }
}