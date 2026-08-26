using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using SobEvents.Application.Commands.Auth;
using SobEvents.Application.DTOs;

namespace SobEvents.Api.Controllers;

/// <summary>
/// Handles user registration, JWT login, token refresh rotation, and antiforgery tokens.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController(ISender mediator, IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>
    /// Registers a new user with the Organizer or Attendee role.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
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

        return Created(string.Empty, result.Data);
    }

    /// <summary>
    /// Authenticates a user and issues short-lived JWT access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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

        return Ok(result.Data);
    }

    /// <summary>
    /// Rotates an expired access token using a single-use refresh token with theft detection.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.AccessToken, request.RefreshToken), ct);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Token Refresh Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(result.Data);
    }

    /// <summary>
    ///  Issues the XSRF-TOKEN cookie for double-submit CSRF defense.
    /// </summary>
    [HttpGet("antiforgery-token")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false, // Must be readable by Angular to send in the X-XSRF-TOKEN header!
            SameSite = SameSiteMode.Lax,
            Secure = false // Set to true in production HTTPS
        });

        return NoContent();
    }
}