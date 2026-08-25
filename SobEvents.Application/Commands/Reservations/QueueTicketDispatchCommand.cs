using MediatR;

namespace SobEvents.Application.Commands.Reservations;

public record QueueTicketDispatchCommand(int ReservationId, int UserId) : IRequest<Guid?>;