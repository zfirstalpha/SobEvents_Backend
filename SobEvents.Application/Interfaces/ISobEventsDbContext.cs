using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Interfaces;

public interface ISobEventsDbContext
{
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<TicketType> TicketTypes { get; }
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}