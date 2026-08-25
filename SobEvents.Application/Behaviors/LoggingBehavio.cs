using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SobEvents.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // MODULE 4 & 7: Structured template logging
        logger.LogInformation("Handling CQRS command/query: {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            // Log execution time in milliseconds
            logger.LogInformation(
                "Handled CQRS {RequestName} successfully in {ElapsedMilliseconds}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds
            );

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex, 
                "CQRS {RequestName} failed after {ElapsedMilliseconds}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}