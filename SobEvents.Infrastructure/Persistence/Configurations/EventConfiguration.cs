using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        

        builder.Property(e=>e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e=>e.Location).HasMaxLength(200).IsRequired();

        builder.HasQueryFilter(e=>!e.IsDeleted);

        builder.HasMany(e => e.TicketTypes)
        .WithOne(t => t.Event)
        .HasForeignKey(t => t.EventId)
        .OnDelete(DeleteBehavior.Restrict);

    }
}