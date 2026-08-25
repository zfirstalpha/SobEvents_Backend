using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.Reservations;

public record CreateReservationCommand(
    int TicketTypeId, 
    int Quantity, 
    int UserId
) : IRequest<ReservationResult>;