using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Persistence.Context;

namespace SobEvents.Infrastructure.Persistence.SeedData;

public static class DbSeeder
{
    public static async Task SeedAsync(
        SobEventsDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        CancellationToken ct = default)
    {
        // 1. Auto-apply any pending migrations
        await context.Database.MigrateAsync(ct);

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
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "SobEvents",
                LastName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Organizer");
            }
        }

        // 4. MODULE 5: Seed Sample Published Events with Ticket Types
        if (!await context.Events.AnyAsync(ct))
        {
            var sampleEvents = new List<Event>
            {
                new()
                {
                    Name = "Addis Tech Summit 2026",
                    Description = "The largest annual gathering of software engineers, tech founders, and venture capitalists in East Africa.",
                    StartDate = DateTime.UtcNow.AddDays(14),
                    EndDate = DateTime.UtcNow.AddDays(16),
                    Location = "Millennium Hall, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "Early Bird Pass", Price = 35.00m, Quantity = 150, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), IsActive = true },
                        new() { Name = "Standard Attendee", Price = 65.00m, Quantity = 400, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(14), IsActive = true },
                        new() { Name = "VIP All-Access Pass", Price = 180.00m, Quantity = 50, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(14), IsActive = true }
                    }
                },
                new()
                {
                    Name = "Great Ethiopian Music Festival",
                    Description = "Experience live performances from the top contemporary and traditional artists under the stars.",
                    StartDate = DateTime.UtcNow.AddDays(25),
                    EndDate = DateTime.UtcNow.AddDays(26),
                    Location = "Ghion Hotel Gardens, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "General Admission", Price = 25.00m, Quantity = 1000, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(25), IsActive = true },
                        new() { Name = "VIP Golden Circle", Price = 90.00m, Quantity = 150, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(25), IsActive = true }
                    }
                },
                new()
                {
                    Name = "East Africa Startup Pitch Night",
                    Description = "Watch 10 high-growth fintech and healthtech startups pitch live to international angel investors.",
                    StartDate = DateTime.UtcNow.AddDays(30),
                    EndDate = DateTime.UtcNow.AddDays(30).AddHours(4),
                    Location = "Ethiopian Skylight Hotel, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1515187029135-18ee286d815b?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "Founder Ticket", Price = 20.00m, Quantity = 120, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(29), IsActive = true },
                        new() { Name = "Investor Pass", Price = 120.00m, Quantity = 40, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(29), IsActive = true }
                    }
                },
                new()
                {
                    Name = "Addis Specialty Coffee Expo 2026",
                    Description = "Taste award-winning single-origin coffees from Yirgacheffe and Sidama. Meet master roasters and baristas.",
                    StartDate = DateTime.UtcNow.AddDays(45),
                    EndDate = DateTime.UtcNow.AddDays(47),
                    Location = "UNECA Conference Center, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "Public Tasting Pass", Price = 15.00m, Quantity = 600, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(44), IsActive = true },
                        new() { Name = "Industry Buyer Pass", Price = 75.00m, Quantity = 100, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(44), IsActive = true }
                    }
                },
                new()
                {
                    Name = "Full-Stack Web & Cloud Architecture Workshop",
                    Description = "A hands-on, deep-dive masterclass on building microservices with .NET 10, PostgreSQL, and Angular 22.",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    Location = "ALX Tech Hub, Bole, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1531482615713-2afd69097998?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "Student Pass", Price = 10.00m, Quantity = 40, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(6), IsActive = true },
                        new() { Name = "Professional Developer", Price = 45.00m, Quantity = 60, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(6), IsActive = true }
                    }
                },
                new()
                {
                    Name = "East Africa Design & Creative Arts Gala",
                    Description = "Celebrating the brightest digital artists, UX designers, and creative directors in contemporary African design.",
                    StartDate = DateTime.UtcNow.AddDays(60),
                    EndDate = DateTime.UtcNow.AddDays(60).AddHours(6),
                    Location = "Entoto Park Gallery, Addis Ababa",
                    ImageUrl = "https://images.unsplash.com/photo-1501281668745-f7f57925c3b4?auto=format&fit=crop&w=1200&q=80",
                    Status = "Published",
                    OrganizerId = adminUser.Id,
                    TicketTypes = new List<TicketType>
                    {
                        new() { Name = "General Admission", Price = 30.00m, Quantity = 250, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(59), IsActive = true },
                        new() { Name = "VIP Table of 4", Price = 200.00m, Quantity = 20, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(59), IsActive = true }
                    }
                }
            };

            context.Events.AddRange(sampleEvents);
            await context.SaveChangesAsync(ct);
        }
    }
}