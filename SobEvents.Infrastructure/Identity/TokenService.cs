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

    public async Task<AuthResponseDto> GenerateTokensAsync(AppUser user, CancellationToken ct = default)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
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
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        // Generate Cryptographically Random Refresh Token
        var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            JwtId = jwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token lives for 7 days
            IsUsed = false,
            IsRevoked = false
        };

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(ct);

        return new AuthResponseDto(accessToken, refreshTokenString, expiresAt);
    }

    public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        // 1. Get Principal from expired access token (ignore expiration during validation)
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return new AuthResult(false, "Invalid access token.", null);
        }

        var jwtId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var userIdString = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(jwtId) || !int.TryParse(userIdString, out var userId))
        {
            return new AuthResult(false, "Invalid token claims.", null);
        }

        // 2. Find the Refresh Token in the database
        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (storedToken == null)
        {
            return new AuthResult(false, "Refresh token does not exist.", null);
        }

        // 3. MODULE 11 SESSION 2: THEFT DETECTION!
        // If the token was ALREADY USED, someone is trying to reuse a consumed token!
        if (storedToken.IsUsed)
        {
            // Revoke ALL refresh tokens for this user immediately
            var userTokens = await context.RefreshTokens.Where(r => r.UserId == userId).ToListAsync(ct);
            foreach (var t in userTokens) t.IsRevoked = true;
            await context.SaveChangesAsync(ct);

            return new AuthResult(false, "Security Alert: Refresh token reuse detected. All sessions terminated.", null);
        }

        // 4. Verify token validity
        if (storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow || storedToken.JwtId != jwtId)
        {
            return new AuthResult(false, "Refresh token is invalid or expired.", null);
        }

        // 5. ROTATE TOKEN: Mark current token as used
        storedToken.IsUsed = true;
        await context.SaveChangesAsync(ct);

        // 6. Issue a fresh pair
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new AuthResult(false, "User not found.", null);

        var newTokens = await GenerateTokensAsync(user, ct);
        return new AuthResult(true, null, newTokens);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            ValidateLifetime = false // Ignore expiration so we can read claims from expired token!
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }
}