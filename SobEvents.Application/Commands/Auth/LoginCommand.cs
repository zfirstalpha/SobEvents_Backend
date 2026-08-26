using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;