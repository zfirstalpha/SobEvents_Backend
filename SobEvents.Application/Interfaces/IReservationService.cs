using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResult> ReserveTicketsAsync(int ticketTypeId, CreateReservationRequest request, int userId, CancellationToken ct = default);
    Task<ReservationResponseDto?> GetReservationByIdAsync(int id, int userId, CancellationToken ct = default);
    Task<List<ReservationResponseDto>> GetReservationsByUserAsync(int userId, CancellationToken ct = default);
    Task<bool> CancelReservationAsync(int id, int userId, CancellationToken ct = default);
}