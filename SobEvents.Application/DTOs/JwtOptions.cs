using System.ComponentModel.DataAnnotations;

namespace SobEvents.Application.DTOs;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters long for HMAC-SHA256 security.")]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationInMinutes { get; set; } = 15;
}