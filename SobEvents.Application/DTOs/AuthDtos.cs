namespace SobEvents.Application.DTOs;

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