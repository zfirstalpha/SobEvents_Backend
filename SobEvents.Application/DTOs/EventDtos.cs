using System.ComponentModel.DataAnnotations;
namespace SobEvents.Application.DTOs;

//this is what the client will send us when creating event
public record CreateEventRequest(
   [Required (ErrorMessage = "Event name is required")]
   [MaxLength(100, ErrorMessage = "Event name cannot exceed 100 characters")]
   string Name, 
    [MaxLength(500, ErrorMessage = "Event description cannot exceed 500 characters")]
    string Description, 
    [Required(ErrorMessage = "Event date is required")]
    DateTime Date, 
    [Required(ErrorMessage = "Event location is required")]
    [MaxLength(200, ErrorMessage = "Event location cannot exceed 200 characters")]
    string Location
);

// This is  what we will send back to the frontend:
public record EventResponseDto(
    int Id, 
    string Name, 
    string Description, 
    DateTime Date, 
    string Location, 
    string Status
);