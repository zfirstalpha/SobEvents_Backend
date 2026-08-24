using Microsoft.AspNetCore.Mvc.Filters;

namespace SobEvents.Api.Filters;

public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;

        // Structured logging template 
        logger.LogInformation("SobEvents API call: {Method} {Route}", method, route);
    }

     public void OnActionExecuted(ActionExecutedContext context)
    {
        var status = context.HttpContext.Response.StatusCode;

        logger.LogInformation("SobEvents API response: {StatusCode}", status);
    }
}