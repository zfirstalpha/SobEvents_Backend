using SobEvents.Domain.Entities;

namespace SobEvents.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(AppUser user, CancellationToken ct = default);
}