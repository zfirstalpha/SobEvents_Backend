namespace SobEvents.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }

    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}