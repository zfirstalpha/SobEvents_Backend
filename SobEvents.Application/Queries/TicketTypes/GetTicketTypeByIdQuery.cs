using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.TicketTypes;

public record GetTicketTypeByIdQuery(int EventId, int Id) : IRequest<TicketTypeResponseDto?>;