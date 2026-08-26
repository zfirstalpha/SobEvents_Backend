using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.Auth;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResult>;