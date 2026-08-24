using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries;

public record GetEventsPagedQuery(PagedRequestDto Request) : IRequest<PagedResponseDto<EventResponseDto>>;