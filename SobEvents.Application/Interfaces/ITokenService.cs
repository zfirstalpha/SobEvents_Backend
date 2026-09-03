using SobEvents.Application.DTOs;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Interfaces;

public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(AppUser user, CancellationToken ct = default);
    Task<AuthResult> RotateRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}