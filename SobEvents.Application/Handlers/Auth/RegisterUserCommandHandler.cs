using MediatR;
using Microsoft.AspNetCore.Identity;
using SobEvents.Application.Commands.Auth;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.Auth;

public class RegisterUserCommandHandler(
    UserManager<AppUser> userManager,
    ITokenService tokenService) 
    : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResult(false, "User with this email already exists.", null);
        }

        var user = new AppUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return new AuthResult(false, errors, null);
        }

        await userManager.AddToRoleAsync(user, request.Role);

        var (accessToken, refreshToken) = await tokenService.GenerateTokensAsync(user, cancellationToken);
        var userDto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName, request.Role);

        return new AuthResult(true, null, userDto, accessToken, refreshToken);
    }
}