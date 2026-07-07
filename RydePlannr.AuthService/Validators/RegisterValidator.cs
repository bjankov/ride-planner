using FluentValidation;
using RydePlannr.AuthService.DTOs;

namespace RydePlannr.AuthService.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Korisničko ime je obavezno.")
            .MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 znaka.")
            .MaximumLength(50).WithMessage("Korisničko ime ne smije biti duže od 50 znakova.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Korisničko ime smije sadržavati samo slova, brojeve i _.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email je obavezan.")
            .EmailAddress().WithMessage("Email nije u ispravnom formatu.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lozinka je obavezna.")
            .MinimumLength(8).WithMessage("Lozinka mora imati najmanje 8 znakova.")
            .Matches("[A-Z]").WithMessage("Lozinka mora sadržavati najmanje jedno veliko slovo.")
            .Matches("[a-z]").WithMessage("Lozinka mora sadržavati najmanje jedno malo slovo.")
            .Matches("[0-9]").WithMessage("Lozinka mora sadržavati najmanje jedan broj.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Lozinka mora sadržavati najmanje jedan poseban znak.");
    }
}
