using MediatR;
using SobEvents.Application.DTOs;

namespace SobEvents.Application.Commands.Auth;

public record RegisterUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Role
) : IRequest<AuthResult>;