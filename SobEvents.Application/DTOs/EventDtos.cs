using System.ComponentModel.DataAnnotations;
namespace SobEvents.Application.DTOs;

// wat the client will send us when creating event
public record CreateEventRequest(
   [Required (ErrorMessage = "Event name is required")]
   [MaxLength(100, ErrorMessage = "Event name cannot exceed 100 characters")]
   string Name, 
    [MaxLength(500, ErrorMessage = "Event description cannot exceed 500 characters")]
    string Description, 

[Required(ErrorMessage = "Event start date is required")]
    DateTime StartDate, 
    [Required (ErrorMessage = "Event end date is required")]
     DateTime EndDate, 
   
    [Required(ErrorMessage = "Event location is required")]
    [MaxLength(200, ErrorMessage = "Event location cannot exceed 200 characters")]
    string Location,
    string? ImageUrl
);

// This is  what we will send back to the frontend:
public record EventResponseDto(
    int Id, 
    string Name, 
    string Description, 
    DateTime StartDate, 
    DateTime EndDate, 
    string Location, 
    string? ImageUrl,
    string Status
);