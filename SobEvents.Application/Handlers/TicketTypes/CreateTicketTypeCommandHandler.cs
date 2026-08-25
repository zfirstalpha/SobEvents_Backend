using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.TicketTypes;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.TicketTypes;

public class CreateTicketTypeCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache)
    : IRequestHandler<CreateTicketTypeCommand, TicketTypeResponseDto?>
{
    public async Task<TicketTypeResponseDto?> Handle(CreateTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var eventExists = await context.Events
            .AnyAsync(e => e.Id == request.EventId && e.OrganizerId == request.OrganizerId, cancellationToken);

        if (!eventExists) return null;

        var ticketType = new TicketType
        {
            EventId = request.EventId,
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        context.TicketTypes.Add(ticketType);
        await context.SaveChangesAsync(cancellationToken);

        // Invalidate tickets cache tag!
        await cache.RemoveByTagAsync("tickets", cancellationToken);

        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{ticketType.EventId}/tickets/{ticketType.Id}", "self", "GET"),
            new($"/api/v1/events/{ticketType.EventId}/tickets/{ticketType.Id}", "update", "PUT"),
            new($"/api/v1/events/{ticketType.EventId}/tickets/{ticketType.Id}", "delete", "DELETE"),
            new($"/api/v1/tickets/{ticketType.Id}/reservations", "reserve", "POST")
        };

        return new TicketTypeResponseDto(
            ticketType.Id, ticketType.EventId, ticketType.Name, ticketType.Price,
            ticketType.Quantity, ticketType.Quantity, ticketType.StartDate, ticketType.EndDate,
            ticketType.IsActive, links);
    }
}