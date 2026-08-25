using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Scalar.AspNetCore;
using SobEvents.Infrastructure.Persistence.Context;
using SobEvents.Infrastructure.Persistence.SeedData;
using SobEvents.Application.Interfaces;
using SobEvents.Api.Middlewares;
using SobEvents.Api.Filters;
using SobEvents.Application.Commands.Events;
using SobEvents.Application.Behaviors;
using MediatR;
using FluentValidation;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SobEvents.Infrastructure.Services;
using SobEvents.Infrastructure.BackgroundServices;
var builder = WebApplication.CreateBuilder(args);

//di container validation (catch captive dependencies at startup)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// controller service
builder.Services.AddControllers(options =>
{
    //register acgion filter globally for all ocntroller
    options.Filters.Add<AuditLogFilter>();
}
);

 //api versioning 
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

//Token-Bucket Rate Limiter Configuration
builder.Services.AddRateLimiter(options =>
{
    // Return honest 429 status code
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // 1. General Limiter for Browsing (Events & Ticket Types)
    options.AddTokenBucketLimiter(policyName: "general-limiter", opt =>
    {
        opt.TokenLimit = 20;               // Maximum capacity in bucket
        opt.QueueLimit = 0;                // Do not buffer requests; reject immediately
        opt.TokensPerPeriod = 5;           // Add 5 tokens...
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // ...every 10 seconds
        opt.AutoReplenishment = true;
    });

    // 2. Strict Limiter for Booking Reservations (Anti-Scalper Defense)
    options.AddTokenBucketLimiter(policyName: "booking-limiter", opt =>
    {
        opt.TokenLimit = 5;                // Maximum 5 rapid clicks
        opt.QueueLimit = 0;
        opt.TokensPerPeriod = 1;           // Add 1 token...
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // ...every 10 seconds
        opt.AutoReplenishment = true;
    });

    // Format the 429 response into standard RFC 7807 ProblemDetails
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Type = "https://datatracker.ietf.org/doc/html/rfc6585#section-4",
            Detail = "Rate limit exceeded. You are making requests too quickly. Please wait and try again.",
            Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
        };

        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
    };
});


// problemdetails & exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();



// dbcontext
builder.Services.AddDbContext<SobEventsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISobEventsDbContext>(provider => 
    provider.GetRequiredService<SobEventsDbContext>());

//  Register MediatR, FluentValidation, and Pipeline Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly);

      // 1. Logging Behavior runs first (measures total execution time)
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // Register the Validation Behavior pipeline globally!
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateEventCommand).Assembly);    

// HybridCache with stampede protection
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// service registrations
//singletton so it can be shared across all request
builder.Services.AddSingleton<ITicketJobQueue, TicketJobQueue>();
//autonomous backgorund service
builder.Services.AddHostedService<TicketProcessingWorker>();
// builder.Services.AddScoped<IEventService, EventService>();
// builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
// builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

//  global exception middleware (must be at the top of the pipeline)
app.UseExceptionHandler();
app.UseStatusCodePages(); // translates 404s/401s into problemdetails automatically

//http request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Generates the JSON blueprint
    app.MapScalarApiReference(); // Draws Scalar UI
}

// run seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SobEventsDbContext>();
    await DbSeeder.SeedAsync(context);
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();


app.UseAuthorization();

app.MapControllers();

app.Run();
