using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
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

    if (exception is ValidationException validationException)
        {
            var validationErrors = validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

        var validationProblemDetails = new HttpValidationProblemDetails(validationErrors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://datatracker.ietf.org/doc/html/rfc7807",
                Detail = "See the errors field for details.",
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            validationProblemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);

            return true;
        }



        // generic 500 internal server error
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