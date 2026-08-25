namespace SobEvents.Application.DTOs;

public record TicketJob(
    Guid JobId,
    int ReservationId,
    string UserEmail,
    string EventName,
    int Quantity,
    DateTime QueuedAt
);