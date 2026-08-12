using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Data;

public class SobEventsDbContext : DbContext
{
    public SobEventsDbContext(DbContextOptions<SobEventsDbContext> options) : base(options)
    {
    }

    // These DbSets become your PostgreSQL tables
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // This method is where we configure "Clean" database rules
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        // Any time we query Events, EF Core will silently add "WHERE IsDeleted = false"
        modelBuilder.Entity<Event>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}