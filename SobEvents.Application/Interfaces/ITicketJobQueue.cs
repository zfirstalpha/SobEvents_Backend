using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface ITicketJobQueue
{
    ValueTask EnqueueAsync(TicketJob job, CancellationToken ct = default);
    IAsyncEnumerable<TicketJob> ReadAllAsync(CancellationToken ct);
}