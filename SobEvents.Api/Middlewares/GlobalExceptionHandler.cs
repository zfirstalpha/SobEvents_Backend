using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SobEvents.Api.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
         // Structured template logging with correlation ID
        logger.LogError(
            exception, 
            "Unhandled exception occurred on {Method} {Path}: {Message}", 
            httpContext.Request.Method, 
            httpContext.Request.Path, 
            exception.Message
        );

        // Standardized RFC 7807 ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Detail = "An unexpected error occurred while processing your request. Please quote the trace ID to support.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

         // Attach the correlation trace ID so developers can find the crash in the server logs
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to tell ASP.NET Core that this exception has been safely handled
        return true; 
    }

}