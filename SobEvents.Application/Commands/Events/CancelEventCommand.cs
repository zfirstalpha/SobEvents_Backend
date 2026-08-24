using MediatR;

namespace SobEvents.Application.Commands.Events;

public record CancelEventCommand(int Id, int OrganizerId) : IRequest<bool>;