using MediatR;
using Microsoft.AspNetCore.Identity;
using SobEvents.Application.Commands.Auth;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.Auth;

public class LoginCommandHandler(
    UserManager<AppUser> userManager,
    ITokenService tokenService) 
    : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResult(false, "Invalid credentials.", null);
        }

        // Cryptographic password verification
        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return new AuthResult(false, "Invalid credentials.", null);
        }

        var tokens = await tokenService.GenerateTokensAsync(user, cancellationToken);
        return new AuthResult(true, null, tokens);
    }
}