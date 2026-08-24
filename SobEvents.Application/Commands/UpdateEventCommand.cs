using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands;

public record UpdateEventCommand(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? ImageUrl,
    int OrganizerId
) : IRequest<EventResponseDto?>;