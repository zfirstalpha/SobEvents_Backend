namespace SobEvents.Application.Interfaces;


public interface IEventsHubService
{
    Task BroadcastTicketsRemainingUpdatedAsync(int ticketTypeId, int availableQuantity, CancellationToken ct = default);
    Task BroadcastEventStatusChangedAsync(int eventId, string newStatus, CancellationToken ct = default);
}