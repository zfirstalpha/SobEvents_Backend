using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.TicketTypes;

public record UpdateTicketTypeCommand(
    int Id,
    int EventId,
    string Name,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    int OrganizerId
) : IRequest<TicketTypeResponseDto?>;