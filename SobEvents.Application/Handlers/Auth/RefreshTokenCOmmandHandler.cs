using MediatR;
using SobEvents.Application.Commands.Auth;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers.Auth;

public class RefreshTokenCommandHandler(ITokenService tokenService) 
    : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await tokenService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);
    }
}