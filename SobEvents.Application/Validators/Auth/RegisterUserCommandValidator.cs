using FluentValidation;
using SobEvents.Application.Commands.Auth;

namespace SobEvents.Application.Validators.Auth;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
        RuleFor(x => x.Role).Must(r => r is "Organizer" or "Attendee")
            .WithMessage("Role must be either 'Organizer' or 'Attendee'.");
    }
}