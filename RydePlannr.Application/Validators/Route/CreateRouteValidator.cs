using FluentValidation;
using RydePlannr.Application.DTOs.Route;

namespace RydePlannr.Application.Validators.Route;

public class CreateRouteValidator : AbstractValidator<CreateRouteDto>
{
    public CreateRouteValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv rute je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 znakova.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Opis ne smije biti duži od 500 znakova.")
            .When(x => x.Description is not null);

        RuleFor(x => x.DistanceKm)
            .GreaterThan(0).WithMessage("Udaljenost mora biti veća od 0.");

        RuleFor(x => x.ElevationGainMeters)
            .GreaterThanOrEqualTo(0).WithMessage("Uspon ne smije biti negativan.");

        RuleFor(x => x.Surface)
            .IsInEnum().WithMessage("Nepoznata podloga rute.");

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("Nepoznata težina rute.");

        RuleFor(x => x.StartLocationId)
            .GreaterThan(0).WithMessage("Početna lokacija je obavezna.");

        RuleFor(x => x.EndLocationId)
            .GreaterThan(0).WithMessage("Završna lokacija je obavezna.");
    }
}