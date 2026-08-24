using MediatR;

namespace SobEvents.Application.Commands.TicketTypes;

public record DeleteTicketTypeCommand(int Id, int EventId, int OrganizerId) : IRequest<bool>;