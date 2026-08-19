using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Persistence.Context;

namespace SobEvents.Infrastructure.Persistence.SeedData;

public static class DbSeeder
{
    public static async Task SeedAsync(SobEventsDbContext context, CancellationToken ct= default)
    {
        await context.Database.MigrateAsync(ct);//apply any pending migration automatically on starutup

        if(!await context.Users.AnyAsync(ct))
        {
            var testOrganizer = new User
            {
                Username="test_organizer",
                Email="testorganizer@gmail.com",
                FirstName ="Test",
                LastName="Organizer",
                PasswordHash="Test@123",
                Role="Organizer"
                
            };
            context.Users.Add(testOrganizer);
            await context.SaveChangesAsync(ct);
        }

    }
}