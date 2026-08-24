using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.TicketTypes;

public record CreateTicketTypeCommand(
    int EventId,
    string Name,
    decimal Price,
    int Quantity,
    DateTime StartDate,
    DateTime EndDate,
    int OrganizerId
) : IRequest<TicketTypeResponseDto?>;