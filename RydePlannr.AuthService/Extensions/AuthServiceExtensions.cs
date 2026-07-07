using FluentValidation;
using RydePlannr.AuthService.Services.Implementations;
using RydePlannr.AuthService.Services.Interfaces;

namespace RydePlannr.AuthService.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthenticationService>();
        services.AddValidatorsFromAssemblyContaining<AuthenticationService>();

        return services;
    }
}
