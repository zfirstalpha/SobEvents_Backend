using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SobEvents.Domain.Entities;

namespace SobEvents.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasQueryFilter(e=>!e.IsDeleted);

        builder.Property(e=>e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e=>e.Location).HasMaxLength(200).IsRequired();
    }
}