using MediatR;

namespace SobEvents.Application.Commands;

public record CancelEventCommand(int Id, int OrganizerId) : IRequest<bool>;