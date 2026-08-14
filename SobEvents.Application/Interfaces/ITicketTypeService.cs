using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface ITicketTypeService
{
    Task<TicketTypeResponseDto?> CreateTicketTypeAsync(int eventId, CreateTicketTypeRequest request, int organizerId);
    Task<List<TicketTypeResponseDto>> GetTicketTypesByEventAsync(int eventId);
}