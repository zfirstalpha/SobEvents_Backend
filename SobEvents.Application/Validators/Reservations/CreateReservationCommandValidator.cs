using FluentValidation;
using SobEvents.Application.Commands.Reservations;

namespace SobEvents.Application.Validators.Reservations;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 10)
            .WithMessage("You can only reserve between 1 and 10 tickets per transaction.");
    }
} 