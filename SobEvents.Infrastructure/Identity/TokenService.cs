using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Domain.Entities;
using SobEvents.Infrastructure.Persistence.Context;

namespace SobEvents.Infrastructure.Identity;

public class TokenService(
    IOptions<JwtOptions> jwtOptions,
    UserManager<AppUser> userManager,
    SobEventsDbContext context) 
    : ITokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(AppUser user, CancellationToken ct = default)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, jwtId)
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes), // 15min
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        // Generate 7-day cryptographically random refresh token
        var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            JwtId = jwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // 7-day lifetime
            IsUsed = false,
            IsRevoked = false
        };

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(ct);

        return (accessToken, refreshTokenString);
    }

    public async Task<AuthResult> RotateRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (storedToken == null)
        {
            return new AuthResult(false, "Invalid refresh token.", null);
        }

        // THEFT DETECTION: If token was already used, revoke all sessions for this user!
        if (storedToken.IsUsed)
        {
            var userTokens = await context.RefreshTokens
                .Where(r => r.UserId == storedToken.UserId)
                .ToListAsync(ct);

            foreach (var t in userTokens) t.IsRevoked = true;
            await context.SaveChangesAsync(ct);

            return new AuthResult(false, "Security Alert: Refresh token reuse detected. All sessions terminated.", null);
        }

        if (storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return new AuthResult(false, "Refresh token is expired or revoked.", null);
        }

        // SINGLE-USE CONSUMPTION: Invalidate the current token
        storedToken.IsUsed = true;
        await context.SaveChangesAsync(ct);

        // Issue new token pair
        var user = storedToken.User;
        var roles = await userManager.GetRolesAsync(user);
        var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user, ct);

        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Attendee");
        return new AuthResult(true, null, userDto, newAccessToken, newRefreshToken);
    }
}