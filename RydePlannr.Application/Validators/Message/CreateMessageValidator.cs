using FluentValidation;
using RydePlannr.Application.DTOs.Message;

namespace RydePlannr.Application.Validators.Message;

public class CreateMessageValidator : AbstractValidator<CreateMessageDto>
{
    public CreateMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Sadržaj poruke je obavezan.")
            .MaximumLength(1000).WithMessage("Poruka ne smije biti duža od 1000 znakova.");
    }
}