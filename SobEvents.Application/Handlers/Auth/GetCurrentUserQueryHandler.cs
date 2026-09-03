using MediatR;
using Microsoft.AspNetCore.Identity;
using SobEvents.Application.DTOs;
using SobEvents.Application.Queries.Auth;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Handlers.Auth;

public class GetCurrentUserQueryHandler(UserManager<AppUser> userManager)
    : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return null;

        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Attendee");
    }
}