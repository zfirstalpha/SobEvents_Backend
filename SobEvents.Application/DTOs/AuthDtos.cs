namespace SobEvents.Application.DTOs;

public record RegisterRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Role // "Organizer" or "Attendee"
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

public record AuthResult(
    bool IsSuccess,
    string? ErrorMessage,
    AuthResponseDto? Data
);