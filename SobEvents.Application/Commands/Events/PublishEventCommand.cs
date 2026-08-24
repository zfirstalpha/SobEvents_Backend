using MediatR;

namespace SobEvents.Application.Commands.Events;

public record PublishEventResult(bool Success, string? ErrorMessage);

public record PublishEventCommand(int Id, int OrganizerId) : IRequest<PublishEventResult>;