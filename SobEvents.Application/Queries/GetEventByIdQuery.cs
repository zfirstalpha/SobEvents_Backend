using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries;

public record GetEventByIdQuery(int Id) : IRequest<EventResponseDto?>;