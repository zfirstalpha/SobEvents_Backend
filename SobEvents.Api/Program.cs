using Microsoft.EntityFrameworkCore;
using SobEvents.Infrastructure.Persistence.Context;
using SobEvents.Infrastructure.Persistence.SeedData;
using SobEvents.Infrastructure.Services;
using SobEvents.Application.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//di container validation (catch captive dependencies at startup)
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// controller service
builder.Services.AddControllers();
builder.Services.AddOpenApi();



// dbcontext
builder.Services.AddDbContext<SobEventsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// service registrations
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

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
