using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SobEvents.Application.Commands.TicketTypes;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.TicketTypes;

public class UpdateTicketTypeCommandHandler(
    ISobEventsDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateTicketTypeCommand, TicketTypeResponseDto?>
{
    public async Task<TicketTypeResponseDto?> Handle(UpdateTicketTypeCommand request, CancellationToken cancellationToken)
    {
        var ticket = await context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Reservations)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.EventId == request.EventId && t.Event.OrganizerId == request.OrganizerId, cancellationToken);

        if (ticket == null) return null;

        ticket.Name = request.Name;
        ticket.Price = request.Price;
        ticket.Quantity = request.Quantity;
        ticket.StartDate = request.StartDate;
        ticket.EndDate = request.EndDate;
        ticket.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        // Invalidate tickets cache tag on update!
        await cache.RemoveByTagAsync("tickets", cancellationToken);

        var reserved = ticket.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.Quantity);
        var available = ticket.Quantity - reserved;

        var links = new List<LinkDto>
        {
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "self", "GET"),
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "update", "PUT"),
            new($"/api/v1/events/{ticket.EventId}/tickets/{ticket.Id}", "delete", "DELETE")
        };

        if (available > 0 && ticket.IsActive)
        {
            links.Add(new($"/api/v1/tickets/{ticket.Id}/reservations", "reserve", "POST"));
        }

        return new TicketTypeResponseDto(
            ticket.Id, ticket.EventId, ticket.Name, ticket.Price, ticket.Quantity,
            available, ticket.StartDate, ticket.EndDate, ticket.IsActive, links);
    }
}