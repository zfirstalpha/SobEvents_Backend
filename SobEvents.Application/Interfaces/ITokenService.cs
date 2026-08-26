using SobEvents.Application.DTOs;
using SobEvents.Domain.Entities;

namespace SobEvents.Application.Interfaces;

public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokensAsync(AppUser user, CancellationToken ct = default);
    Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);
}