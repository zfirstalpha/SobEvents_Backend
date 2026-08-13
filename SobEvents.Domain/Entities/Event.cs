namespace SobEvents.Domain.Entities;

public class Event
{
    public int Id { get; set; } 
    public int OrganizerId { get; set; } // Foreign key to User
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    public required string Location { get; set; }

    public string? ImageUrl { get; set; } //  image for the event

    public DateTime StartDate { get; set; } // Start date and time of the event
    public DateTime EndDate { get; set; } // End date and time of the event
    public string Status { get; set; } = "Draft"; // Draft, Published, Cancelled
    public bool IsDeleted { get; set; } = false; // Soft delete flag

    public User Organizer { get; set; } = null!;
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
}