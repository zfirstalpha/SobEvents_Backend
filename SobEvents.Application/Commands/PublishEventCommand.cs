using MediatR;

namespace SobEvents.Application.Commands;

public record PublishEventResult(bool Success, string? ErrorMessage);

public record PublishEventCommand(int Id, int OrganizerId) : IRequest<PublishEventResult>;