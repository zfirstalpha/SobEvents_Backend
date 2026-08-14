using System.ComponentModel.DataAnnotations;

namespace SobEvents.Application.DTOs;

public class CreateTicketTypeRequest : IValidatableObject
{
    [Required (ErrorMessage = "Ticket Name is required (e.g., Standard,VIP, Early Bird, etc.)")]
    [MaxLength(50, ErrorMessage = "Ticket Name cannot exceed 50 characters.")]
    public string Name { get; init; } = string.Empty;

    [Range(0,1000000, ErrorMessage = "Price must be between 0 and 1,000,000.")]
    public decimal Price { get; init; }

     [Range(1, 100000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; init; }

    [Required] public DateTime StartDate { get; init; }
    [Required] public DateTime EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "Ticket sales End Date must be after Start Date.", 
                new[] { nameof(EndDate) }
            );
        }
    }
}

public record TicketTypeResponseDto(
    int Id,
    int EventId,
    string Name,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);