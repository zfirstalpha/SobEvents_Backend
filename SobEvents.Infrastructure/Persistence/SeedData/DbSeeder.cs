using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Persistence.Context;

namespace SobEvents.Infrastructure.Persistence.SeedData;

public static class DbSeeder
{
    public static async Task SeedAsync(SobEventsDbContext context,
    UserManager<AppUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
         CancellationToken ct= default)
    {
        await context.Database.MigrateAsync(ct);//apply any pending migration automatically on starutup

        // 2. Seed Default Roles (Module 11 Session 1)
        string[] roles = ["Organizer", "Attendee"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

// 3. Seed Default Organizer Admin User
        var adminEmail = "admin@sobevents.com";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "SobEvents",
                LastName = "Admin",
                EmailConfirmed = true
            };
            // Creates user with cryptographic PBKDF2 password hash!
            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Organizer");
            }
        }

    }
}