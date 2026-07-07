using FluentValidation;
using RydePlannr.Application.DTOs.RideEvent;

namespace RydePlannr.Application.Validators;

public class CreateRideEventValidator : AbstractValidator<CreateRideEventDto>
{
    public CreateRideEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naziv događaja je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 znakova.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Opis ne smije biti duži od 1000 znakova.")
            .When(x => x.Description is not null);

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Vrijeme početka je obavezno.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Događaj mora biti u budućnosti.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("Vrijeme završetka mora biti nakon vremena početka.")
            .When(x => x.EndTime is not null);

        RuleFor(x => x.CutoffMinutes)
            .GreaterThan(0).WithMessage("Vrijeme za završetak mora biti veće od 0 minuta.")
            .When(x => x.CutoffMinutes is not null);

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("Broj sudionika mora biti veći od 0.")
            .LessThanOrEqualTo(1000).WithMessage("Broj sudionika ne smije biti veći od 1000.");

        RuleFor(x => x.RouteId)
            .GreaterThan(0).WithMessage("Ruta je obavezna.");

        RuleFor(x => x.RideTypeId)
            .GreaterThan(0).WithMessage("Tip vožnje je obavezan.");
    }
}