using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(r => r.Token).HasMaxLength(200).IsRequired();
        builder.Property(r => r.JwtId).HasMaxLength(100).IsRequired();

        builder.HasIndex(r => r.Token).IsUnique();

        // If a user is deleted, their refresh tokens are deleted with them
        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}