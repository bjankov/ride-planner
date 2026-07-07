using FluentValidation;
using RydePlannr.AuthService.DTOs;

namespace RydePlannr.AuthService.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token je obavezan.");
    }
}
