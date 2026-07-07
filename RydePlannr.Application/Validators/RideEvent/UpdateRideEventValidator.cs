using FluentValidation;
using RydePlannr.Application.DTOs.RideEvent;

namespace RydePlannr.Application.Validators;

public class UpdateRideEventValidator : AbstractValidator<UpdateRideEventDto>
{
    public UpdateRideEventValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 znakova.")
            .When(x => x.Title is not null);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("Vrijeme završetka mora biti nakon vremena početka.")
            .When(x => x.EndTime is not null && x.StartTime is not null);

        RuleFor(x => x.CutoffMinutes)
            .GreaterThan(0).WithMessage("Vrijeme za završetak mora biti veće od 0 minuta.")
            .When(x => x.CutoffMinutes is not null);

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).WithMessage("Broj sudionika mora biti veći od 0.")
            .When(x => x.MaxParticipants is not null);
    }
}