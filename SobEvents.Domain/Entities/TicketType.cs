namespace SobEvents.Domain.Entities;

public class TicketType
{
    public int Id { get; set; }
    public int EventId { get; set; } // Foreign Key to Event
    
    public required string Name { get; set; } // "VIP", "Early Bird", etc.
    public decimal Price { get; set; }
    public int Quantity { get; set; } // Total capacity
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Event Event { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}