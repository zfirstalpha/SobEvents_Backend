using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        //enforce unique username and email
        builder.HasIndex(u=>u.Username).IsUnique();
        builder.HasIndex(u=>u.Email).IsUnique();
    }
}