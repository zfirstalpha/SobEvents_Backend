using Microsoft.AspNetCore.Identity;

namespace SobEvents.Domain.Entities;


public class AppUser : IdentityUser<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    // Navigation Properties
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}