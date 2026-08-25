using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Reservations;

public record GetReservationByIdQuery(int Id, int UserId) : IRequest<ReservationResponseDto?>;