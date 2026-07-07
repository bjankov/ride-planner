using FluentValidation;
using RydePlannr.Application.DTOs.Club;

namespace RydePlannr.Application.Validators.Club;

public class CreateClubValidator : AbstractValidator<CreateClubDto>
{
    public CreateClubValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv kluba je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 znakova.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Opis ne smije biti duži od 500 znakova.")
            .When(x => x.Description is not null);
    }
}