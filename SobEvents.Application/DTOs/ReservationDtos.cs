using System.ComponentModel.DataAnnotations;

namespace SobEvents.Application.DTOs;
public record CreateReservationRequest(
    [Range(1, 10, ErrorMessage = "You can only reserve between 1 and 10 tickets at a time.")]
    int Quantity
);
public record ReservationResponseDto(
    int Id,
    int TicketTypeId,
    int UserId,
    int Quantity,
    DateTime ReservedAt,
    DateTime ExpiryDate,
    string Status,
    List<LinkDto> Links  //for hateoas
);

//custom resullt so the service can tell the controller why it failed
public record ReservationResult(
    bool IsSuccess, 
    string? ErrorMessage, 
    ReservationResponseDto? Reservation
);