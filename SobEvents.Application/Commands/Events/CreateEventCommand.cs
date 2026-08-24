using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.Events;

public record CreateEventCommand(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? ImageUrl,
    int OrganizerId
) : IRequest<EventResponseDto>;