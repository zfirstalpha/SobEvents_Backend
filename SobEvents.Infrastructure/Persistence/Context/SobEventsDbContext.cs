using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;
using SobEvents.Application.Interfaces;
namespace SobEvents.Infrastructure.Persistence.Context;

public class SobEventsDbContext :DbContext, ISobEventsDbContext
{
    

public SobEventsDbContext(DbContextOptions<SobEventsDbContext> options):base(options)
    {
        
    }
     public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SobEventsDbContext).Assembly);
    }
    
}
