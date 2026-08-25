using System.Threading.Channels;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Infrastructure.Services;

public class TicketJobQueue : ITicketJobQueue
{
    private readonly Channel<TicketJob> _channel;

    public TicketJobQueue()
    {
        //  Bounded channel prevents out-of-memory crashes
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<TicketJob>(options);
    }

    public async ValueTask EnqueueAsync(TicketJob job, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(job, ct);
    }

    public IAsyncEnumerable<TicketJob> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}