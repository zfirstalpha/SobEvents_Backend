using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Events;

//  event queries to the logged-in OrganizerId
public record GetOrganizerEventsQuery(int OrganizerId, PagedRequestDto Request) 
    : IRequest<PagedResponseDto<EventResponseDto>>;