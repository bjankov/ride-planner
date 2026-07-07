using FluentValidation;
using RydePlannr.Application.DTOs.Location;

namespace RydePlannr.Application.Validators.Location;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv lokacije je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 znakova.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Zemljopisna širina mora biti između -90 i 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Zemljopisna dužina mora biti između -180 i 180.");
    }
}