using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Events;

public record GetEventsPagedQuery(PagedRequestDto Request) : IRequest<PagedResponseDto<EventResponseDto>>;