namespace SobEvents.Application.DTOs;

public record UserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string Role
);

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

public record AuthResult(
    bool IsSuccess,
    string? ErrorMessage,
    UserDto? User,
    string? AccessToken = null,
    string? RefreshToken = null
);