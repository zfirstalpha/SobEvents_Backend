using MediatR;

namespace SobEvents.Application.Commands.Reservations;

public record CancelReservationCommand(int Id, int UserId) : IRequest<bool>;