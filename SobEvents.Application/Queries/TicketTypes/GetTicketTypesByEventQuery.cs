using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.TicketTypes;

public record GetTicketTypesByEventQuery(int EventId) : IRequest<List<TicketTypeResponseDto>>;