using Microsoft.EntityFrameworkCore;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Interfaces;

public interface ISobEventsDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<TicketType> TicketTypes { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}