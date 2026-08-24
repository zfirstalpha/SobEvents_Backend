using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Events;

public record GetEventByIdQuery(int Id) : IRequest<EventResponseDto?>;