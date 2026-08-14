using SobEvents.Application.DTOs;

namespace SobEvents.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResult> ReserveTicketsAsync(int ticketTypeId, CreateReservationRequest request, int userId);
}