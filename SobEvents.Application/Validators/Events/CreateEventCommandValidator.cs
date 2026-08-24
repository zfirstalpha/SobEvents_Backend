using FluentValidation;
using SobEvents.Application.Commands.Events;

namespace SobEvents.Application.Validators.Events;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event Name is required.")
            .MaximumLength(100).WithMessage("Event Name cannot exceed 100 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        // Cross-field rule!
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after Start date.");
    }
}