using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface ITicketTypeService
{
    Task<TicketTypeResponseDto?> CreateTicketTypeAsync(int eventId, CreateTicketTypeRequest request, int organizerId,CancellationToken ct=default);
    Task<List<TicketTypeResponseDto>> GetTicketTypesByEventAsync(int eventId,CancellationToken ct=default);
       Task<TicketTypeResponseDto?> GetTicketTypeByIdAsync(int id, CancellationToken ct = default);
    Task<TicketTypeResponseDto?> UpdateTicketTypeAsync(int id, UpdateTicketTypeRequest request, int organizerId, CancellationToken ct = default);
    Task<bool> DeleteTicketTypeAsync(int id, int organizerId, CancellationToken ct = default);
}