using FluentValidation;
using RydePlannr.AuthService.DTOs;

namespace RydePlannr.AuthService.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email je obavezan.")
            .EmailAddress().WithMessage("Email nije u ispravnom formatu.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lozinka je obavezna.");
    }
}
