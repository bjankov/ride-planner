using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RydePlannr.Domain.Interfaces;
using RydePlannr.Infrastructure.Persistence;

namespace RydePlannr.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("RydePlannr.Infrastructure")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}