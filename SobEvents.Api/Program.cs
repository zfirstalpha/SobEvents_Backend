using Microsoft.EntityFrameworkCore;
using SobEvents.Infrastructure.Data;
using SobEvents.Infrastructure.Services;
using SobEvents.Application.Interfaces;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();



// Add DbContext
builder.Services.AddDbContext<SobEventsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register our Service
builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Generates the JSON blueprint
    app.MapScalarApiReference(); // Draws the beautiful Scalar UI
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SobEventsDbContext>();
    
    // create o4ganizer if there is no user
    if (!context.Users.Any())
    {
        context.Users.Add(new SobEvents.Domain.Entities.User
        {
            Username = "test_organizer",
            Email = "test@gmail.com",
            FirstName = "TestOrganizer",
            LastName = "Org",
            PasswordHash ="Test@123",
            Role = "Organizer"
        });
        context.SaveChanges();
    }
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
