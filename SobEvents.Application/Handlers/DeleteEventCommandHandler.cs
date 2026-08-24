using MediatR;
using Microsoft.EntityFrameworkCore;
using SobEvents.Application.Commands;
using SobEvents.Application.Interfaces;

namespace SobEvents.Application.Handlers;

public class DeleteEventCommandHandler(ISobEventsDbContext context)
    : IRequestHandler<DeleteEventCommand, bool>
{
    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var evt = await context.Events
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.OrganizerId == request.OrganizerId, cancellationToken);

        if (evt == null) return false;

        //  Soft Delete
        evt.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}