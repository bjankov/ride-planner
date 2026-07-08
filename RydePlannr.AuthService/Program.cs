using Microsoft.OpenApi.Models;
using RydePlannr.AuthService.Extensions;
using RydePlannr.AuthService.Filters;
using RydePlannr.AuthService.Middleware;
using RydePlannr.Infrastructure.Extensions;
using Serilog;

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
    Log.Information("Pokretanje RydePlannr.AuthService...");

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
    builder.Services.AddAuthServices();

    // Dev-only: lets the local api-tester frontend (opened via file:// or a static server)
    // call this service from the browser. Not used for anything internet-facing.
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "RydePlannr Auth Service",
            Version = "v1",
            Description = "Servis zadužen za registraciju, prijavu i izdavanje JWT tokena"
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseCors();

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
