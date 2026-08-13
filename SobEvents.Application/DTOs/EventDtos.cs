namespace SobEvents.Application.DTOs;

//this is what the client will send us when creating event
public record CreateEventRequest(
    string Name, 
    string Description, 
    DateTime Date, 
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