using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SobEvents.Application.Interfaces;

namespace SobEvents.Infrastructure.BackgroundServices;

public class TicketProcessingWorker(
    ITicketJobQueue queue,
    ILogger<TicketProcessingWorker> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TicketProcessingWorker started. Listening to background Channel<T>...");

        // Continuously reads from the non-blocking channel stream
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation(
                    "Processing background job {JobId} for Reservation #{ReservationId} ({EventName}, {Quantity} tickets) -> Dispatching to {UserEmail}",
                    job.JobId, job.ReservationId, job.EventName, job.Quantity, job.UserEmail);

                // Simulate heavy background work (e.g. PDF rendering, QR code generation, SMTP dispatch)
                await Task.Delay(2000, stoppingToken);

                logger.LogInformation("Successfully completed background job {JobId}", job.JobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing background job {JobId}", job.JobId);
            }
        }
    }
}