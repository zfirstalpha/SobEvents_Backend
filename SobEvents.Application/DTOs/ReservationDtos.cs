using System.ComponentModel.DataAnnotations;

namespace SobEvents.Application.DTOs;
public record CreateReservationRequest(
    [Range(1, 10, ErrorMessage = "You can only reserve between 1 and 10 tickets at a time.")]
    int Quantity
);

public record SubmitPaymentProofRequest(
    [Required] [MaxLength(100)]
    string TransactionReference
);

public record RejectReservationRequest(
    [MaxLength(200)]
    string? Reason
);

public record ReservationResponseDto(
    int Id,
    int TicketTypeId,
    string TicketTypeName,
    string EventName,
    int UserId,
    string AttendeeName,
    string AttendeeEmail,
    int Quantity,
    DateTime ReservedAt,
    DateTime ExpiryDate,
    string Status,
    string? TransactionReference,
    string? RejectionReason,
    List<LinkDto> Links
);

//custom resullt so the service can tell the controller why it failed
public record ReservationResult(
    bool IsSuccess, 
    string? ErrorMessage, 
    ReservationResponseDto? Reservation
);