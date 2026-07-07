using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RydePlannr.API.Extensions;
using RydePlannr.API.Filters;
using RydePlannr.API.Middleware;
using RydePlannr.Application.Extensions;
using RydePlannr.Infrastructure.Extensions;
using RydePlannr.Infrastructure.Persistence;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Pokretanje RydePlannr API-ja...");

    var builder = WebApplication.CreateBuilder(args);

   builder.Host.UseSerilog((context, services, configuration) =>
       configuration.ReadFrom.Configuration(context.Configuration)
           .WriteTo.Console()
           .WriteTo.File(
               path: "Logs/log-.txt",
               rollingInterval: RollingInterval.Day,
               retainedFileCountLimit: 30,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers(options =>
        options.Filters.Add<FluentValidationFilter>());
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddSwaggerWithJwt();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<LoggingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplikacija se srušila pri pokretanju.");
}
finally
{
    await Log.CloseAndFlushAsync();
}