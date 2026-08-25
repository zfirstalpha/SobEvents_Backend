using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Reservations;

public record GetMyReservationsQuery(int UserId) : IRequest<List<ReservationResponseDto>>;