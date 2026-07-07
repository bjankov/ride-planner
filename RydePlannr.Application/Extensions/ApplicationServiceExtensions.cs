using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RydePlannr.Application.Mappings;
using RydePlannr.Application.Services.Implementations;
using RydePlannr.Application.Services.Interfaces;

namespace RydePlannr.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRideEventService, RideEventService>();
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<ILocationService, LocationService>();

        services.AddAutoMapper(cfg => cfg.AddProfile<RideProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<ClubProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<RouteProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<UserProfile>());
        
        services.AddValidatorsFromAssemblyContaining<UserService>();

        return services;
    }
}