using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SobEvents.Application.Interfaces;

namespace SobEvents.Infrastructure.Identity;

//  Registered as Scoped so each HTTP request gets its own user context!
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var idClaim = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? _user?.FindFirst("sub")?.Value;

            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Email => _user?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => _user?.IsInRole(role) ?? false;
}