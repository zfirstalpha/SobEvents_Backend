using MediatR;

namespace SobEvents.Application.Commands;

public record DeleteEventCommand(int Id, int OrganizerId) : IRequest<bool>;