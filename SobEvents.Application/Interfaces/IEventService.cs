using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface IEventService
{
    Task<EventResponseDto> CreateEventAsync(CreateEventRequest request, int organizerId);
   Task<PagedResponseDto<EventResponseDto>> GetAllEventsAsync(PagedRequestDto request);
   Task<EventResponseDto?> GetEventByIdAsync(int id);
}