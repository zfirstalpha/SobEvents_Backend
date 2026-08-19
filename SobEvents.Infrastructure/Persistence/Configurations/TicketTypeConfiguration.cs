using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Configurations;

public class TicketTypeConfiguration :IEntityTypeConfiguration<TicketType>
{
    public  void Configure (EntityTypeBuilder<TicketType> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Price).HasPrecision(18,2);

        builder.HasMany(t=>t.Reservations)
        .WithOne( r=> r.TicketType)
        .HasForeignKey(r => r.TicketTypeId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}