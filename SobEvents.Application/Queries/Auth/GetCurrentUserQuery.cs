using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Queries.Auth;

public record GetCurrentUserQuery(int UserId) : IRequest<UserDto?>;