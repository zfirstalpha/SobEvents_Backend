using FluentValidation;
using SobEvents.Application.Commands.TicketTypes;

namespace SobEvents.Application.Validators.TicketTypes;

public class CreateTicketTypeCommandValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).InclusiveBetween(0, 100000);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("Ticket sales End Date must be after Start Date.");
    }
}