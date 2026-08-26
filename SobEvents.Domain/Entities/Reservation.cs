namespace SobEvents.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int TicketTypeId { get; set; } // Foreign Key
    public int UserId { get; set; } // Foreign Key
    
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } // For the hours timeout feature
    public string Status { get; set; } = "Reserved"; // Reserved, Paid, Cancelled

    public TicketType TicketType { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}