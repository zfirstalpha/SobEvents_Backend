namespace SobEvents.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Token { get; set; } // Cryptographic random string
    public required string JwtId { get; set; } // Matches the 'jti' claim of the access token
    
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }

    // Navigation Property
    public AppUser User { get; set; } = null!;
}