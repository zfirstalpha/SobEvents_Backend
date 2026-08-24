using MediatR;

namespace SobEvents.Application.Commands.Events;

public record DeleteEventCommand(int Id, int OrganizerId) : IRequest<bool>;