using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface IEventService
{
    Task<EventResponseDto> CreateEventAsync(CreateEventRequest request, int organizerId);
    Task<List<EventResponseDto>> GetAllEventsAsync();
}