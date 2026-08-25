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

app.UseAuthorization();

app.MapControllers();

app.Run();
