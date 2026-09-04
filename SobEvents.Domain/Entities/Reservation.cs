namespace SobEvents.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int TicketTypeId { get; set; }
    public int UserId { get; set; } 
    
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } 
    public string Status { get; set; } = "Reserved"; // Reserved, Paid, Cancelled

    public string? TransactionReference
    {
        get; set;
    }
     public string? RejectionReason { get; set; }

    public TicketType TicketType { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}