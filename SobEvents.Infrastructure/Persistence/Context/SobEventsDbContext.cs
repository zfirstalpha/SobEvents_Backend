using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Context;

// MODULE 11 SESSION 1: IdentityDbContext manages all security tables (Users, Roles, Claims)
public class SobEventsDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>, ISobEventsDbContext
{
    public SobEventsDbContext(DbContextOptions<SobEventsDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must call base.OnModelCreating to configure the Identity schema
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SobEventsDbContext).Assembly);
    }
}