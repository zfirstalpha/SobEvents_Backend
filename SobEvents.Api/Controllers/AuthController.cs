using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.Commands.Auth;
using SobEvents.Application.DTOs;
using SobEvents.Application.Interfaces;
using SobEvents.Application.Queries.Auth;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Handles pure HttpOnly cookie authentication, session restoration, and token rotation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController(
    ISender mediator, 
    ITokenService tokenService, 
    ICurrentUserService currentUser,
    IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>
    /// Registers a new user, writes HttpOnly auth cookies, and returns UserDto (0 tokens in body).
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(
            request.Username, request.Email, request.FirstName,
            request.LastName, request.Password, request.Role
        );

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        // Set BOTH Access and Refresh Tokens in HttpOnly Cookies
        SetAuthCookies(result.AccessToken!, result.RefreshToken!);

        return Created(string.Empty, result.User);
    }

    /// <summary>
    /// Authenticates a user, writes HttpOnly auth cookies, and returns UserDto (0 tokens in body).
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Set BOTH Access and Refresh Tokens in HttpOnly Cookies
        SetAuthCookies(result.AccessToken!, result.RefreshToken!);

        return Ok(result.User);
    }

    /// <summary>
    /// Rotates tokens using the incoming HttpOnly refreshToken cookie (NO REQUEST BODY!).
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // 1. Strictly obtain the refresh token from the HttpOnly cookie
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Refresh Failed",
                Detail = "Missing refresh token cookie.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var result = await tokenService.RotateRefreshTokenAsync(refreshToken, ct);

        if (!result.IsSuccess)
        {
            // Clear cookies if compromised/expired
            DeleteAuthCookies();
            return Unauthorized(new ProblemDetails
            {
                Title = "Refresh Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status401Unauthorized
            });
        }

        // Write newly rotated HttpOnly cookies
        SetAuthCookies(result.AccessToken!, result.RefreshToken!);

        return Ok(result.User);
    }

    /// <summary>
    /// Restores the current authenticated user profile on application startup.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var user = await mediator.Send(new GetCurrentUserQuery(currentUser.UserId!.Value), ct);
        if (user == null) return Unauthorized();
        return Ok(user);
    }

    /// <summary>
    /// Logs out the user and deletes both HttpOnly auth cookies.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        DeleteAuthCookies();
        return NoContent();
    }

    /// <summary>
    /// Issues the XSRF-TOKEN cookie for double-submit CSRF defense.
    /// </summary>
    [HttpGet("antiforgery-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false, // Must be readable by Angular for X-XSRF-TOKEN header!
            SameSite = SameSiteMode.Lax,
            Secure = false // True in production HTTPS
        });

        return NoContent();
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        // 1. 15-Minute Access Token Cookie
        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Path = "/"
        });

        // 2. 7-Day Refresh Token Cookie
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to true in production HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        });
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
    }
}