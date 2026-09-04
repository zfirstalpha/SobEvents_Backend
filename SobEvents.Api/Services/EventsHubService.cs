using Microsoft.AspNetCore.SignalR;
using SobEvents.Api.Hubs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Api.Services;

// f9r signalR
public class EventsHubService(IHubContext<EventsHub> hubContext) : IEventsHubService
{
    public async Task BroadcastTicketsRemainingUpdatedAsync(int ticketTypeId, int availableQuantity, CancellationToken ct = default)
    {
        await hubContext.Clients.All.SendAsync("TicketsRemainingUpdated", ticketTypeId, availableQuantity, cancellationToken: ct);
    }

    public async Task BroadcastEventStatusChangedAsync(int eventId, string newStatus, CancellationToken ct = default)
    {
        await hubContext.Clients.All.SendAsync("EventStatusChanged", eventId, newStatus, cancellationToken: ct);
    }
}