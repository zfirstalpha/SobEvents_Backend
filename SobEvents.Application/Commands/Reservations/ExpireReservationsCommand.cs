using MediatR;

namespace SobEvents.Application.Commands.Reservations;

// Returns the integer count of how many abandoned reservations were cancelled
public record ExpireReservationsCommand() : IRequest<int>;