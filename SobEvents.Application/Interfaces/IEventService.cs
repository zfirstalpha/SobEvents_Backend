using System.Data;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface IEventService
{
      Task<EventResponseDto> CreateEventAsync(CreateEventRequest request, int organizerId, CancellationToken ct = default);
    Task<PagedResponseDto<EventResponseDto>> GetAllEventsAsync(PagedRequestDto request, CancellationToken ct = default);
    Task<EventResponseDto?> GetEventByIdAsync(int id, CancellationToken ct = default);
    Task<EventResponseDto?> UpdateEventAsync(int id, CreateEventRequest request, int organizerId, CancellationToken ct = default);
    Task<bool> DeleteEventAsync(int id, int organizerId, CancellationToken ct = default);
    
    //state transition
    Task<(bool Success, string? ErrorMessage)> PublishEventAsync(int id, int organizerId, CancellationToken ct = default);
    Task<bool> CancelEventAsync(int id, int organizerId, CancellationToken ct = default);
}